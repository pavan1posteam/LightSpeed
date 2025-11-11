using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LightspeedRetail_Api.clsLightspeedXAPI.clsLIghtspeedxProductList;

namespace LightspeedRetail_Api
{
    public class clsLightspeedXAPI
    {
        private readonly int StoreId;
        private readonly decimal tax;
        private readonly string BaseUrl;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string RefreshToken;
        public clsLightspeedXAPI(int _StoreId, decimal _tax, string _BaseUrl, string _ClientId, string _ClientSecret, string _RefreshToken)
        {
            StoreId = _StoreId;
            tax = _tax;
            BaseUrl = _BaseUrl;
            ClientId = _ClientId;
            ClientSecret = _ClientSecret;
            RefreshToken = _RefreshToken;
            Console.WriteLine("Generating Lightspeed " + StoreId + " Product File....");
            Console.WriteLine("Generating Lightspeed " + StoreId + " Fullname File....");
        }

        public async Task RunAsync()
        {
            try
            {
                await LightspeedXAPI_Products(StoreId, tax, BaseUrl, ClientId, ClientSecret, RefreshToken);                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task LightspeedXAPI_Products(int storeid, decimal tax, string BaseUrl, string ClientId, string ClientSecret, string RefreshToken)
        {
            List<ProductResponse> Items = new List<ProductResponse>();
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            try
            {
                var ItemResult = await LightspeedXSetting(BaseUrl, storeid, tax, ClientId, ClientSecret, RefreshToken);

                string Personaltoken = "lsxs_pt_B8dYv7xDjJeAKkmV5wNEpb5dvrdV4Aot";
                List<Root> plist = await getAllProducts(BaseUrl, Personaltoken);

                List<clsLIghtspeedxProductList.LightspeedxProductModel> prodList = new List<clsLIghtspeedxProductList.LightspeedxProductModel>();
                List<clsLIghtspeedxProductList.LightspeedxFullNameProductModel> fullNameList = new List<clsLIghtspeedxProductList.LightspeedxFullNameProductModel>();

                try
                {
                    var completeList = (from a in ItemResult
                                        join b in plist on a.source_id equals b.source_id
                                        let upcCodes = b?.product_codes?.Where(pc => pc.type == "UPC" && pc.code.All(char.IsDigit)).Select(pc => pc.code).ToList()
                                        where upcCodes != null && upcCodes.Any()
                                        where upcCodes.All(upc => upc.All(char.IsDigit))
                                        select new
                                        {
                                            storeid = storeid,
                                            upc = upcCodes,
                                            qty = a.inventory?.FirstOrDefault()?.count,
                                            sku = b?.sku ?? a.source_id,
                                            pack = 1,
                                            StoreProductName = a.name,
                                            StoreDescription = a.name,
                                            price = a.price,
                                            sprice = 0,
                                            start = "",
                                            end = "",
                                            tax = tax,
                                            altupc1 = upcCodes.FirstOrDefault(),
                                            altupc2 = "",
                                            altupc3 = "",
                                            altupc4 = "",
                                            altupc5 = "",
                                            active = b.active,
                                            pcat = b.product_category?.category_path?.ElementAtOrDefault(1)?.name,
                                        }).ToList();

                    completeList.RemoveAll(r => r.active == false);

                    foreach (var item in completeList)
                    {
                        try
                        {
                            clsLIghtspeedxProductList.LightspeedxProductModel prod = new clsLIghtspeedxProductList.LightspeedxProductModel();
                            clsLIghtspeedxProductList.LightspeedxFullNameProductModel fullName = new clsLIghtspeedxProductList.LightspeedxFullNameProductModel();
                            clsLIghtspeedxProductList.Root ItemList = new clsLIghtspeedxProductList.Root();
                            var prc = "";
                            var number = item.upc.ToList();
                            if (number.Count > 1)
                            {

                            }
                            number = number.OrderByDescending(n => n.Length).Distinct().ToList();


                            var a = number.FirstOrDefault();
                            var b = number.LastOrDefault();


                            prod.upc = "#" + (b.Length < 6 ? a : b);
                            fullName.upc = prod.upc;
                            prod.altupc1 = "#" + a.ToString();
                            if (prod.upc == prod.altupc1)
                            {
                                prod.altupc1 = "";

                            }
                            if (item.price <= 0)
                            {
                                continue;
                            }
                            else
                            {
                                prc = item.price.ToString();
                                prod.Price = Convert.ToDecimal(prc);
                                fullName.Price = Convert.ToDecimal(prc);

                            }
                            prod.Qty = Convert.ToInt32(item.qty);

                            prod.StoreID = storeid;


                            prod.sku = '#' + item.sku;
                            fullName.sku = prod.sku;



                            prod.pack = 1;
                            Regex uomRegex = new Regex(@"\b(\d+(\.\d+)?)\s*(ml|ML|oz|OZ|L|l|M|m|0Z)\b", RegexOptions.IgnoreCase);
                            Match match = uomRegex.Match(item.StoreProductName);

                            if (match.Success)
                            {
                                prod.uom = match.Value.ToUpper();
                                fullName.uom = prod.uom;
                            }
                            else
                            {
                                prod.uom = "";
                                fullName.uom = "";
                            }
                            fullName.pack = 1;
                            prod.StoreProductName = item.StoreProductName.ToString();
                            fullName.pname = prod.StoreProductName;
                            prod.StoreDescription = item.StoreProductName.ToString();
                            fullName.pdesc = prod.StoreDescription;
                            prod.sprice = 0;
                            prod.Tax = tax;
                            prod.Start = "";
                            prod.End = "";
                            prod.altupc2 = "";
                            prod.altupc3 = "";
                            prod.altupc4 = "";
                            prod.altupc5 = "";

                            fullName.pcat = item.pcat;
                            fullName.country = "";
                            fullName.region = "";



                            if (prod.Price > 0 && prod.Qty > 0)
                            {
                                prodList.Add(prod);
                                prodList = prodList.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                fullNameList = fullNameList.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                fullNameList.Add(fullName);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                if (prodList.Count > 0 && fullNameList.Count > 0)
                {
                    GenerateCSV.GenerateCSVFile(prodList, "PRODUCT", storeid, BaseDirectory);
                    GenerateCSV.GenerateCSVFile(fullNameList, "FULLNAME", storeid, BaseDirectory);
                    Console.WriteLine();
                    Console.WriteLine("Product FIle Generated For Lightspeedx " + storeid);
                    Console.WriteLine("Fullname FIle Generated For Lightspeedx " + storeid);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Files not generated, No products in the ProductList "+storeid);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<List<Root>> getAllProducts(string BaseUrl, string accessToken)
        {
            List<Root> prod = new List<Root>();
            try
            {
                var client = new RestClient(BaseUrl + "2.0/products?page_size=100000" + ""); //Note:To get more products Increase the page size
                var request = new RestRequest("", Method.Get);
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("cache-control", "no-cache");              
                //request.AddHeader("content-type", "application/x-www-form-urlencoded");
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                    prod = apiResponse.Data;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return prod;
        }

        public async Task<string> getaccesstoken(string BaseUrl, string clientid, string clientsecret, string RefreshToken)
        {
            string AccessToken = "";
            ProductResponse prod = new ProductResponse();
            try
            {
                var client = new RestClient(BaseUrl + "1.0/token" + "");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Cookie", "rguserid=e938db22-bb4f-48a7-8df6-a2f515edbceb; rguuid=true; rgisanonymous=true; vend_retailer=1TCx6za3eEJpuAs8dMJvSRmarDW:T2OOT31NLOIicLonQR6dUgtNqzB");
                request.AddParameter("refresh_token", RefreshToken);
                request.AddParameter("client_id", clientid);
                request.AddParameter("client_secret", clientsecret);
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("redirect_uri", "https://localhost/");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var result = JsonConvert.DeserializeObject<ProductResponse>(content);
                    AccessToken = result.access_token.ToString();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Lightspeedx ");
            }
            return AccessToken;
        }

        public async Task<List<clsLIghtspeedxProductList.Product>> LightspeedXSetting(string BaseUrl, int StoreId, decimal tax, string ClientId, string ClientSecret, string RefreshToken)
        {
            string Url = "";
            clsLIghtspeedxProductList obj = new clsLIghtspeedxProductList();
            int recordsTotal = 200;
            List<clsLIghtspeedxProductList.Product> productList = new List<clsLIghtspeedxProductList.Product>(); ;
            var accesstoken = getaccesstoken(BaseUrl, ClientId, ClientSecret, RefreshToken);
            //BaseUrl ";
            clsLIghtspeedxProductList.Product ItemList = new clsLIghtspeedxProductList.Product();
            int page_size = 200;
            int pgcount = 1;
            try
            {
                for (int pageNo = 0; pageNo <= pgcount; pageNo++)
                {
                    string ApiUrl = BaseUrl + "products" + "?page=" + pageNo + "&page_size=" + page_size + "";
                    ApiUrl = string.IsNullOrEmpty(Url) ? ApiUrl : Url;
                    var client = new RestClient(ApiUrl);
                    var request = new RestRequest("", Method.Get);
                    request.AddHeader("Authorization", "Bearer " + accesstoken);
                    request.AddHeader("cache-control", "no-cache");
                    //request.AddHeader("content-type", "application/json");
                    var response = await client.ExecuteAsync(request);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = response.Content;
                        var result = JsonConvert.DeserializeObject<ProductResponse>(content);
                        var count = result.pagination.results.ToString();
                        var pagesize = result.pagination.page_size.ToString();
                        recordsTotal = Convert.ToInt32(count);
                        page_size = Convert.ToInt32(pagesize);
                        pgcount = result.pagination.pages;
                        productList.AddRange(result.products.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Lightspeedx ");
            }
            return productList;
        }

        public class clsLIghtspeedxProductList
        {
            public class Pagination
            {
                public int results { get; set; }
                public int page { get; set; }
                public int page_size { get; set; }
                public int pages { get; set; }
            }

            public class PriceBookEntry
            {
                public string price_book_name { get; set; }
                public string id { get; set; }
                public string product_id { get; set; }
                public string price_book_id { get; set; }
                public string type { get; set; }
                public string outlet_name { get; set; }
                public string outlet_id { get; set; }
                public string customer_group_name { get; set; }
                public string customer_group_id { get; set; }
                public double price { get; set; }
                public object loyalty_value { get; set; }
                public string tax_id { get; set; }
                public double tax_rate { get; set; }
                public string tax_name { get; set; }
                public int display_retail_price_tax_inclusive { get; set; }
                public string min_units { get; set; }
                public string max_units { get; set; }
                public string valid_from { get; set; }
                public string valid_to { get; set; }
                public double tax { get; set; }
            }

            public class Tax
            {
                public string outlet_id { get; set; }
                public string tax_id { get; set; }
            }

            public class Inventory
            {
                public string outlet_id { get; set; }
                public string outlet_name { get; set; }
                public string count { get; set; }
                public string reorder_point { get; set; }
                public string restock_level { get; set; }
            }

            public class Product
            {
                public string id { get; set; }
                public string source_id { get; set; }
                public string handle { get; set; }
                public bool has_variants { get; set; }
                public string variant_parent_id { get; set; }
                public string variant_option_one_name { get; set; }
                public string variant_option_one_value { get; set; }
                public string variant_option_two_name { get; set; }
                public string variant_option_two_value { get; set; }
                public string variant_option_three_name { get; set; }
                public string variant_option_three_value { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public string description { get; set; }
                public string image { get; set; }
                public string image_large { get; set; }
                public List<object> images { get; set; }
                public string sku { get; set; }
                public string tags { get; set; }
                public string supplier_code { get; set; }
                public string supply_price { get; set; }
                public string account_code_purchase { get; set; }
                public string account_code_sales { get; set; }
                public string button_order { get; set; }
                public List<PriceBookEntry> price_book_entries { get; set; }
                public double price { get; set; }
                public double tax { get; set; }
                public string tax_id { get; set; }
                public double tax_rate { get; set; }
                public string tax_name { get; set; }
                public int display_retail_price_tax_inclusive { get; set; }
                public string updated_at { get; set; }
                public string deleted_at { get; set; }
                public string base_name { get; set; }
                public string brand_id { get; set; }
                public string variant_source_id { get; set; }
                public string brand_name { get; set; }
                public string supplier_name { get; set; }
                public bool track_inventory { get; set; }
                public List<Tax> taxes { get; set; }
                public string type { get; set; }
                public List<Inventory> inventory { get; set; }
            }

            public class ApiResponse
            {
                public List<Root> Data { get; set; }
            }

            public class ProductResponse
            {
                public Pagination pagination { get; set; }
                public string access_token { get; set; }
                public List<Product> products { get; set; }
            }
            public class LightspeedxProductModel
            {
                public int StoreID { get; set; }
                public string upc { get; set; }
                public int Qty { get; set; }
                public string sku { get; set; }
                public int pack { get; set; }
                public string uom { get; set; }
                public string StoreProductName { get; set; }
                public string StoreDescription { get; set; }
                public decimal Price { get; set; }
                public decimal sprice { get; set; }
                public string Start { get; set; }
                public string End { get; set; }
                public decimal Tax { get; set; }
                public string altupc1 { get; set; }
                public string altupc2 { get; set; }
                public string altupc3 { get; set; }
                public string altupc4 { get; set; }
                public string altupc5 { get; set; }

            }
            public class LightspeedxFullNameProductModel
            {
                public string pname { get; set; }
                public string pdesc { get; set; }
                public string upc { get; set; }
                public string sku { get; set; }
                public decimal Price { get; set; }
                public string uom { get; set; }
                public int pack { get; set; }
                public string pcat { get; set; }
                public string pcat1 { get; set; }
                public string pcat2 { get; set; }
                public string country { get; set; }
                public string region { get; set; }
            }

            public class CategoryPath
            {
                public string id { get; set; }
                public string name { get; set; }
            }

            public class Packaging
            {
                public List<object> made_from { get; set; }
                public List<object> breaks_into { get; set; }
            }

            public class ProductCategory
            {
                public string id { get; set; }
                public string name { get; set; }
                public bool leaf_category { get; set; }
                public List<CategoryPath> category_path { get; set; }
            }

            public class ProductCode
            {
                public string id { get; set; }
                public string type { get; set; }
                public string code { get; set; }
            }

            public class ProductSupplier
            {
                public string id { get; set; }
                public string product_id { get; set; }
                public string supplier_id { get; set; }
                public string supplier_name { get; set; }
                public string code { get; set; }
                public double? Price { get; set; }
            }

            public class Root
            {
                public string id { get; set; }
                public string source_id { get; set; }
                public object source_variant_id { get; set; }
                public object variant_parent_id { get; set; }
                public string name { get; set; }
                public string variant_name { get; set; }
                public string handle { get; set; }
                public string sku { get; set; }
                public string supplier_code { get; set; }
                public bool active { get; set; }
                public bool ecwid_enabled_webstore { get; set; }
                public bool has_inventory { get; set; }
                public bool is_composite { get; set; }
                public object description { get; set; }
                public string image_url { get; set; }
                public DateTime created_at { get; set; }
                public DateTime updated_at { get; set; }
                public object deleted_at { get; set; }
                public string source { get; set; }
                public object account_code { get; set; }
                public object account_code_purchase { get; set; }
                public double supply_price { get; set; }
                public long version { get; set; }
                public Type type { get; set; }
                public ProductCategory product_category { get; set; }
                public Supplier supplier { get; set; }
                public object brand { get; set; }
                public List<object> variant_options { get; set; }
                public List<CategoryPath> categories { get; set; }
                public List<object> images { get; set; }
                public List<object> skuImages { get; set; }
                public bool has_variants { get; set; }
                public object variant_count { get; set; }
                public int button_order { get; set; }
                public double? PriceIncludingTax { get; set; }
                public double price_excluding_tax { get; set; }
                public object loyalty_amount { get; set; }
                public List<ProductCode> product_codes { get; set; }
                public List<ProductSupplier> product_suppliers { get; set; }
                public Packaging packaging { get; set; }
                public object weight { get; set; }
                public object weight_unit { get; set; }
                public object length { get; set; }
                public object width { get; set; }
                public object height { get; set; }
                public object dimensions_unit { get; set; }
                public List<object> attributes { get; set; }
                public string supplier_id { get; set; }
                public object brand_id { get; set; }
                public string product_type_id { get; set; }
                public bool is_active { get; set; }
                public string image_thumbnail_url { get; set; }
                public List<object> tag_ids { get; set; }
            }

            public class Supplier
            {
                public string id { get; set; }
                public string name { get; set; }
                public string source { get; set; }
                public string description { get; set; }
                public object deleted_at { get; set; }
                public long version { get; set; }
            }

            public class Type
            {
                public string id { get; set; }
                public string name { get; set; }
                public object deleted_at { get; set; }
                public long version { get; set; }
            }

            public class RootData
            {
                public Data data { get; set; }
            }

            public class Category
            {
                public string id { get; set; }
                public string name { get; set; }
                public string root_category_id { get; set; }
                public object parent_category_id { get; set; }
                public bool leaf_category { get; set; }
                public List<CategoryPath> category_path { get; set; }
                public string PayloadID { get; set; }
            }


            public class Data
            {
                public PageInfo page_info { get; set; }
                public Data data { get; set; }
                public List<Category> categories { get; set; }
            }

            public class PageInfo
            {
                public bool has_next { get; set; }
                public string last_seen { get; set; }
            }
        }
    }
}