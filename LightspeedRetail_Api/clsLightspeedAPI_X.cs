using Newtonsoft.Json;
using RestSharp;
using System;
using System.Linq;
using System.Collections.Generic;
using static LightspeedRetail_Api.clsLightspeedAPI_XProductList;
using System.Configuration;
using LightspeedRetail_Api.Models;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace LightspeedRetail_Api
{
    public class clsLightspeedAPI_X
    {
        private readonly int _storeId;
        private readonly decimal _tax;
        private readonly string _baseUrl;
        private readonly string _accessToken;

        public clsLightspeedAPI_X(int storeId, decimal tax, string baseUrl, string accessToken)
        {
            _storeId = storeId;
            _tax = tax;
            _baseUrl = baseUrl;
            _accessToken = accessToken;
        }
        #region OLD Contructor
        //public clsLightspeedAPI_X(int StoreId, decimal tax, string baseurl, string accesstoken)
        //{
        //    try
        //    {
        //        Console.WriteLine("Generating Lightspeed " + StoreId + " Product File....");
        //        Console.WriteLine("Generating Lightspeed " + StoreId + " Fullname File....");
        //        clsLightspeedx_productsAsync(StoreId, tax, baseurl, accesstoken);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}
        #endregion
        public async Task RunAsync()
        {
            try
            {
                Console.WriteLine("Generating Lightspeed " + _storeId + " Product File....");
                Console.WriteLine("Generating Lightspeed " + _storeId + " Fullname File....");
                await clsLightspeedx_productsAsync(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public async Task clsLightspeedx_productsAsync()
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            //List<Datum> plist = getAllproducts(baseurl, accesstoken);
            //List<Data> Ilist = new List<Data>();

            //for (int i = 0; i < plist.Count; i++)
            //{

            //    var id = plist[i].id;
            //    List<Data> inventory = getAllInventory(baseurl, id, accesstoken);
            //    Ilist.AddRange(inventory);

            //}
            
            List<Datum> plist = await GetAllproducts(_baseUrl, _accessToken);
            List<Task<List<Data>>> tasks;
            
            if (plist.Count > 5000)
            {
                var throttler = new SemaphoreSlim(20); 
                tasks = new List<Task<List<Data>>>();

                foreach (var p in plist)
                {
                    await throttler.WaitAsync();

                    var task = getAllInventory(_baseUrl, p.id, _accessToken).ContinueWith(t =>
                    {
                        throttler.Release();
                        return t.Result;
                    });
                    tasks.Add(task);
                }
            }
            else
            {
                tasks = plist.Select(p => getAllInventory(_baseUrl, p.id, _accessToken)).ToList();
            }
            var results = await Task.WhenAll(tasks);

            // Combine all inventory lists into one
            List<Data> Ilist = results.SelectMany(r => r).ToList();

            //List<> Clist = getAllCategory(baseurl, accesstoken);
            List<Lightspeed_xProductModel> prodlist = new List<Lightspeed_xProductModel>();
            List<Lightspeed_xFullNameProductModel> fulllist = new List<Lightspeed_xFullNameProductModel>();

            try
            {

                var productlist = (from p in plist
                                   join i in Ilist on p.id equals i.product_id
                                   let upcCodes = p?.product_codes
                                   where upcCodes != null && upcCodes.Any() && p.is_active==true
                                   select new
                                   {
                                       storeid = _storeId,
                                       upc = upcCodes,
                                       qty = i.inventory_level,
                                       sku = p?.sku ?? p.source_id,
                                       pack = 1,
                                       StoreProductName = p.name,
                                       StoreDescription = p.name,
                                       price = p.price_excluding_tax,
                                       sprice = 0,
                                       start = "",
                                       end = "",
                                       tax = _tax,
                                       altupc1 = "",
                                       altupc2 = "",
                                       altupc3 = "",
                                       altupc4 = "",
                                       altupc5 = "",
                                       active = p.active,
                                       pcat = p.product_category?.name,
                                       outlet = i.outlet_id
                                   }).ToList();


                foreach (var item in productlist)
                {
                    Lightspeed_xProductModel prod = new Lightspeed_xProductModel();
                    Lightspeed_xFullNameProductModel full = new Lightspeed_xFullNameProductModel();

                    prod.StoreID = _storeId;
                    prod.upc = "#" + item.upc[0].code;
                    full.upc = "#" + item.upc[0].code;


                    prod.sku = "#" + item.sku;
                    full.sku = "#" + item.sku;
                    prod.Qty = item.qty;
                    if (item.price > 0)
                    {
                        prod.Price = (decimal)item.price;
                        full.Price = (decimal)item.price;

                    }
                    prod.sprice = 0;
                    prod.StoreProductName = item.StoreProductName;
                    full.pname = item.StoreProductName;
                    prod.StoreDescription = item.StoreDescription;
                    full.pdesc = item.StoreDescription;
                    prod.Tax = _tax;

                    full.pcat = item.pcat;

                    prod.altupc1 = "";
                    prod.altupc2 = "";
                    prod.altupc3 = "";
                    prod.altupc4 = "";
                    prod.altupc5 = "";
                    prod.Start = "";
                    prod.End = "";
                    prod.pack = 1;
                    full.pack = 1;
                    full.country = "";
                    full.region = "";

                    if (prod.StoreID == 12160 && item.outlet == "06e94082-ed34-11ee-f619-d50768b8c813")
                    {
                       if (prod.Price > 0)
                        {
                            prodlist.Add(prod);
                            fulllist.Add(full);
                        }
                    }
                    else if (prod.StoreID == 12233 && item.outlet == "06326976-9d65-11ec-fa40-f267fe682cf6")
                    {
                        if (prod.Price > 0)
                        {
                            prodlist.Add(prod);
                            fulllist.Add(full);
                        }
                    }
                    else if(prod.StoreID != 12233 && prod.StoreID !=12160)
                    {
                        if (prod.Price > 0)
                        {
                            prodlist.Add(prod);
                            fulllist.Add(full);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (prodlist.Count > 0 && fulllist.Count > 0)
            {
                GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", _storeId, BaseDirectory);
                GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", _storeId, BaseDirectory);
                Console.WriteLine();
                Console.WriteLine("Product FIle Generated For Lightspeedx " + _storeId);
                Console.WriteLine("Fullname FIle Generated For Lightspeedx " + _storeId);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Files not generated, No products in the ProductList " + _storeId);

            }


        }
        public async Task<List<Data>> getAllInventory(string baseUrl, string id, string accessToken)
        {
            List<Data> inventoryList = new List<Data>();
            try
            {
                var client = new RestClient(baseUrl);

                var request = new RestRequest($"2.0/products/{id}/inventory", Method.Get)
                    .AddHeader("Authorization", $"Bearer {accessToken}")
                    .AddHeader("cache-control", "no-cache");
                    //.AddHeader("content-type", "application/x-www-form-urlencoded");

                var response = await client.ExecuteAsync(request); 

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<Roots>(response.Content);
                    inventoryList = apiResponse?.Data ?? new List<Data>();
                }
                else
                {
                    Console.WriteLine($"Inventory API Error (ID: {id}): {response.StatusCode} - {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in getAllInventory (ID: {id}): {ex.Message}");
            }
            return inventoryList;
        }

        #region OLD Version RestSharp earlier 108.0.3
        //public async Task<List<Datum>> GetAllproducts(string BaseUrl, string accessToken)
        //{
        //    List<Datum> prod = new List<Datum>();
        //    try
        //    {
        //        var client = new RestClient(BaseUrl + "2.0/products?page_size=1000000000" + ""); //Note:To get more products Increase the page size
        //        var request = new RestRequest(Method.GET);
        //        request.AddHeader("Authorization", "Bearer " + accessToken);
        //        request.AddHeader("cache-control", "no-cache");
        //        //request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //        var response =await client.ExecuteAsync(request);
        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            var apiResponse = JsonConvert.DeserializeObject<Root>(response.Content);
        //            prod = apiResponse.data;
        //        }
        //        //File.AppendAllText("12097(1).json", response.Content);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }

        //    return prod;
        //}
        //public async Task<List<Data>> getAllInventory(string Baseurl, string id, string accesstoken)
        //{
        //    List<Data> Iprod = new List<Data>();

        //    try
        //    {
        //        //var client = new RestClient(Baseurl + "2.0/inventory?page_size=100000" + "");
        //        var client = new RestClient(Baseurl + "2.0/products/" + id + "/inventory" + "");
        //        var request = new RestRequest(Method.GET);
        //        request.AddHeader("Authorization", "Bearer " + accesstoken);
        //        request.AddHeader("cache-control", "no-cache");
        //        //request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //        IRestResponse response = client.Execute(request);
        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            var apiresponse = JsonConvert.DeserializeObject<Roots>(response.Content);
        //            Iprod = apiresponse.Data;
        //        }
        //        //File.AppendAllText("12097.json", response.Content);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    return Iprod;
        //}
        //public List<> getAllCategory(string Baseurl,string accesstoken)
        //{
        //    List<> cprod = new List<>();

        //    try
        //    {
        //        var client=new RestClient(Baseurl+"2.0/")
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}
        #endregion
        public async Task<List<Datum>> GetAllproducts(string baseUrl, string accessToken)
        {
            List<Datum> prod = new List<Datum>();
            try
            {
                //baseUrl += "2.0/products?page_size=1000000000";
                var client = new RestClient(baseUrl);
                var request = new RestRequest("2.0/products", Method.Get);
                request.AddQueryParameter("page_size", "1000000000"); //Note:To get more products Increase the page size
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("cache-control", "no-cache");
                //request.AddHeader("content-type", "application/x-www-form-urlencoded");

                var response = await client.ExecuteAsync(request); 

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<Root>(response.Content);
                    prod = apiResponse.data;
                }
                else
                {
                    Console.WriteLine($"API Error: {response.StatusCode} - {response.Content}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
            return prod;
        }
    }

    public class clsLightspeedAPI_XProductList
    {
        public class Brand
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public object deleted_at { get; set; }
            public object version { get; set; }
        }

        public class Category
        {
            public string id { get; set; }
            public string name { get; set; }
            public object deleted_at { get; set; }
            public object version { get; set; }
        }

        public class CategoryPath
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public object source_id { get; set; }
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
            public string account_code { get; set; }
            public object account_code_purchase { get; set; }
            public double? supply_price { get; set; }
            public object version { get; set; }
            public Type type { get; set; }
            public ProductCategory product_category { get; set; }
            public Supplier supplier { get; set; }
            public Brand brand { get; set; }
            public List<object> variant_options { get; set; }
            public List<Category> categories { get; set; }
            public List<Image> images { get; set; }
            public List<object> skuImages { get; set; }
            public bool has_variants { get; set; }
            public object variant_count { get; set; }
            public int button_order { get; set; }
            public double? price_including_tax { get; set; }
            public double? price_excluding_tax { get; set; }
            public object loyalty_amount { get; set; }
            public List<ProductCode> product_codes { get; set; }
            public List<ProductSupplier> product_suppliers { get; set; }
            public Packaging packaging { get; set; }
            public double? weight { get; set; }
            public object weight_unit { get; set; }
            public double? length { get; set; }
            public double? width { get; set; }
            public double? height { get; set; }
            public object dimensions_unit { get; set; }
            public List<object> attributes { get; set; }
            public string supplier_id { get; set; }
            public string product_type_id { get; set; }
            public string brand_id { get; set; }
            public bool is_active { get; set; }
            public string image_thumbnail_url { get; set; }
            public List<string> tag_ids { get; set; }
        }

        public class Image
        {
            public string id { get; set; }
            public string url { get; set; }
            public object version { get; set; }
            public Sizes sizes { get; set; }
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
            public double? price { get; set; }
        }

        public class Root
        {
            public List<Datum> data { get; set; }
            public Version version { get; set; }
        }

        public class Sizes
        {
            public string raw { get; set; }
            public string original { get; set; }
            public string sl { get; set; }
            public string sm { get; set; }
            public string ss { get; set; }
            public string st { get; set; }
            public string standard { get; set; }
            public string thumb { get; set; }
        }

        public class Supplier
        {
            public string id { get; set; }
            public string name { get; set; }
            public string source { get; set; }
            public string description { get; set; }
            public object deleted_at { get; set; }
            public object version { get; set; }
        }

        public class Type
        {
            public string id { get; set; }
            public string name { get; set; }
            public object deleted_at { get; set; }
            public object version { get; set; }
        }


        public class Data
        {
            public string id { get; set; }
            public string outlet_id { get; set; }
            public string product_id { get; set; }

            [JsonConverter(typeof(InventoryLevelConverter))]
            public int inventory_level { get; set; }
            public object current_amount { get; set; }
            public object version { get; set; }
            public object deleted_at { get; set; }
            public double? average_cost { get; set; }
            public object reorder_point { get; set; }
            public object reorder_amount { get; set; }
        }

        public class Roots
        {
            public List<Data> Data { get; set; }
            public Version version { get; set; }
        }

        public class Lightspeed_xProductModel
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
        public class Lightspeed_xFullNameProductModel
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

    }
}