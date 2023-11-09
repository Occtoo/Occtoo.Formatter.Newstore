using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Occtoo.Formatter.Newstore.Models;
using Occtoo.Formatter.Newstore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Occtoo.Formatter.Newstore
{
    public class NewStoreExport
    {
        public const string NewstoreContainer = "newstore";
        public const string ProductEntities = "products";
        public const string CategoriesEntities = "categories";

        private static List<string> LocalesList = new List<string> {
            "en-us", "fr", "da", "sv" };
        private readonly IOcctooService _occtooService;
        private readonly IBlobService _blobService;

        public NewStoreExport(IOcctooService occtooService, IBlobService blobService)
        {
            _occtooService = occtooService;
            _blobService = blobService;
        }

        [FunctionName(nameof(NewStoreExport))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
              [Queue("%newStoreQueue%"), StorageAccount("AzureWebJobsStorage")] ICollector<string> newStoreQueue,
              ILogger log)
        {
            try
            {
                var productVariants = await _occtooService.GetProductVariants();
                var filesToZip = CreateCategoryFiles(productVariants);
                filesToZip.AddRange(CreateProductFiles(productVariants));
                foreach (var fileToZip in filesToZip)
                {
                    var fileToZipList = new List<ZipHelper.ZipContentsEntry>() { fileToZip };
                    var fileName = fileToZip.FullPath.Replace(".json", ".zip");
                    var blobTask = await _blobService.UploadAsZipToBlobStorage(fileToZipList, fileName, NewstoreContainer);

                    var newstoreOutboxDto = new NewstoreOutboxDto { FileName = fileName, Locale = fileToZip.Locale };
                    var message = JsonConvert.SerializeObject(newstoreOutboxDto);
                    newStoreQueue.Add(message);
                }

                return new OkObjectResult("done");
            }
            catch (Exception e)
            {
                log.LogError("Fatal error: " + e.Message);
                return new BadRequestObjectResult(new { e, currentDate = DateTime.Now });
            }
        }

        private static List<ZipHelper.ZipContentsEntry> CreateCategoryFiles(List<OcctooProductVariant> productVariants)
        {
            var dicForCategory = GetCategoriesDic(productVariants);
            var itemCategories = new List<ItemCategory>();
            foreach (var keyPairValue in dicForCategory)
            {
                itemCategories.Add(new ItemCategory
                {
                    Description = keyPairValue.Value,
                    Path = keyPairValue.Key
                });
            }

            var filesToZip = new List<ZipHelper.ZipContentsEntry>();
            foreach (var locale in LocalesList)
            {
                var categoryLocale = new Categories
                {
                    Head = new Head
                    {
                        Shop = "storefront-catalog-en",
                        Locale = locale,
                        IsMaster = locale.Equals("en-us")
                    },

                    Items = itemCategories
                };

                var serializedCategory = JsonConvert.SerializeObject(categoryLocale);
                filesToZip.Add(new ZipHelper.ZipContentsEntry
                {
                    Contents = serializedCategory,
                    FullPath = $"{CategoriesEntities}_{locale}.json",
                    Locale = locale
                });
            }

            return filesToZip;
        }

        private static Dictionary<string, string> GetCategoriesDic(List<OcctooProductVariant> productVariants)
        {
            var dicForCategory = new Dictionary<string, string>();
            foreach (var productVariant in productVariants)
            {
                var division = $"{productVariant.Division}";
                if (!dicForCategory.ContainsKey(division))
                {
                    dicForCategory.Add(division, division);
                }

                var typeKey = $"{productVariant.Division} > {productVariant.Type}";
                if (!dicForCategory.ContainsKey(typeKey))
                {
                    dicForCategory.Add(typeKey, productVariant.Type);
                }

                var itemCategoryKey = $"{productVariant.Division} > {productVariant.Type} > {productVariant.ItemCategoryKey}";
                if (!dicForCategory.ContainsKey(itemCategoryKey))
                {
                    dicForCategory.Add(itemCategoryKey, productVariant.ItemCategoryKey);
                }

                var itemName = $"{productVariant.Division} > {productVariant.Type} > {productVariant.ItemCategoryKey} > {productVariant.Name}";
                if (!dicForCategory.ContainsKey(itemName))
                {
                    dicForCategory.Add(itemName, productVariant.Name);
                }
            }

            return dicForCategory;
        }

        private static List<ZipHelper.ZipContentsEntry> CreateProductFiles(List<OcctooProductVariant> productVariants)
        {
            var filesToZip = new List<ZipHelper.ZipContentsEntry>();
            foreach (var locale in LocalesList)
            {
                var serializedProductCatalog = JsonConvert.SerializeObject(CreateProductCatalog(productVariants, locale));
                var localeIdForFile = locale;
                if (locale.ToLower().Equals("en-us"))
                {
                    localeIdForFile = "default";
                }

                filesToZip.Add(new ZipHelper.ZipContentsEntry
                {
                    Contents = serializedProductCatalog,
                    FullPath = $"{ProductEntities}_{localeIdForFile}.json",
                    Locale = localeIdForFile
                });
            }

            return filesToZip;
        }

        private static Catalog CreateProductCatalog(List<OcctooProductVariant> productVariants, string locale)
        {
            var export = new Catalog
            {
                Head = CreateCatalogHead(locale),
                Items = CreateCatalogItemList(productVariants)
            };

            return export;
        }

        private static List<CatalogItem> CreateCatalogItemList(List<OcctooProductVariant> productVariants)
        {
            var items = new List<CatalogItem>();
            foreach (var variant in productVariants)
            {
                items.Add(new CatalogItem
                {
                    ProductId = variant.Id ?? "",
                    VariantGroupId = variant.Number ?? "",
                    Title = variant.Name + variant.Color ?? "",
                    Caption = variant.Name + variant.Color ?? "",
                    Description = variant.Description ?? "",
                    TaxClassId = variant.HsCode ?? "",
                    ShippingWeightUnit = "kg" ?? "",
                    VariationColorValue = variant.Color ?? "",
                    VariationSizeValue = $"EU {variant.SizeEu} / US (W) {variant.SizeUsWomen} / US (M) {variant.SizeUsMen} / UK {variant.SizeUk}" ?? "",
                    ExtendedAttributes = CreateExtendedAttributes(variant),
                    Categories = CreateCategories(variant),
                    Images = variant.Media.Select(m => new Models.Image { Url = m.Url }).ToList(),
                    VariationSizeGender = GetProductGenderKey(variant.ProductGenderKey)
                });
            }

            return items;
        }

        private static string GetProductGenderKey(string productGenderKey)
        {
            if (string.IsNullOrEmpty(productGenderKey))
                return "";

            switch (productGenderKey.ToLower())
            {
                case "men":
                    return "male";
                case "women":
                    return "female";
                case "female":
                    return "female";
                case "unisex":
                    return "unisex";
                default:
                    return "";
            }
        }

        private static List<ExtendedAttributes> CreateExtendedAttributes(OcctooProductVariant variant)
        {
            var list = new List<ExtendedAttributes>();

            AddExtendedAttribute(list, "ProductGiftWrap", "False");
            AddExtendedAttribute(list, "ProductInternalName", variant.InternalName);
            AddExtendedAttribute(list, "ProductType", variant.Type);

            return list;
        }

        private static void AddExtendedAttribute(ICollection<ExtendedAttributes> list, string name, string value)
        {
            list.Add(new ExtendedAttributes
            {
                Name = name,
                Value = value ?? ""
            });
        }

        private static List<Category> CreateCategories(OcctooProductVariant variant)
        {
            return new List<Category>
            {
                new Category
                {
                    IsMain = true,
                    Path = $"{variant.Division} > {variant.Type} > {variant.ItemCategoryKey} > {variant.Name}"
                }
            };
        }

        private static CatalogHead CreateCatalogHead(string locale)
        {
            var head = new CatalogHead
            {
                Shop = "storefront-catalog-en",
                Catalog = "storefront-catalog-en",
                ShopDisplayName = $"Storefront Catalog {locale}",
                Locale = locale,
                SearchableAttributes = CreateSrchFlterableAttribute("product_id", "$.product_id"),
                FilterableAttributes = CreateSrchFlterableAttribute("Gender", "$.variation_size_gender"),
                IsMaster = locale.Equals("en-us")
            };

            return head;
        }

        private static SrchFlterAbleAttributes[] CreateSrchFlterableAttribute(string name, string path)
        {
            return new[] {
                new SrchFlterAbleAttributes {
                    Name = name,
                    Path = path
                }
            };
        }
    }
}
