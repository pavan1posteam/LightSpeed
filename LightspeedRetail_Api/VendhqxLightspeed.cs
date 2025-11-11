using LightspeedRetail_Api.Model;
using LightspeedRetail_Api.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LightspeedRetail_Api
{
    class VendhqxLightspeed
    {
        private readonly int StoreId;
        private readonly decimal tax;
        private readonly string BaseUrl;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string RefreshToken;
        public VendhqxLightspeed(int _StoreId, decimal _tax, string _BaseUrl, string _ClientId, string _ClientSecret, string _RefreshToken)
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
                await VendhqxLightspeed_Products(StoreId, tax, BaseUrl, ClientId, ClientSecret, RefreshToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task<string> getaccesstoken(string BaseUrl, string clientid, string clientsecret, string RefreshToken)
        {
            string AccessToken = "";
            Product prod = new Product();
            try
            {
                var client = new RestClient(BaseUrl + "1.0/token" + "");
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("code", "5alwWshKiUUNniUBNFZys_v5lrjodcIM99mZyStD");
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
                    var result = JsonConvert.DeserializeObject<Product>(content);
                    AccessToken = result.access_token.ToString();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " VendHQ ");
            }
            return AccessToken;
        }
        //public async Task<List<Stock>> getStocks(string BaseUrl, int StoreId, string ClientId, string ClientSecret, string RefreshToken)
        //{
        //    string Url = "";
        //    List<Stock> Stocklist = new List<Stock>();
        //    var accesstoken = getaccesstoken(BaseUrl, ClientId, ClientSecret, RefreshToken);
        //    StockResult ItemList = new StockResult();
        //    try
        //    {
        //        string ApiUrl = BaseUrl + "2.0/" + "inventory";
        //        ApiUrl = string.IsNullOrEmpty(Url) ? ApiUrl : Url;
        //        var client = new RestClient(ApiUrl);
        //        var request = new RestRequest("", Method.Get);
        //        request.AddHeader("Authorization", "Bearer " + accesstoken);
        //        request.AddHeader("cache-control", "no-cache");
        //        //request.AddHeader("content-type", "application/json");
        //        request.AddParameter("refresh_token", RefreshToken);
        //        request.AddParameter("client_id", ClientId);
        //        request.AddParameter("client_secret", ClientSecret);
        //        request.AddParameter("grant_type", "refresh_token");
        //        var response = await client.ExecuteAsync(request);
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            var content = response.Content;
        //            Stock stockResult = (Stock)JsonConvert.DeserializeObject(content, typeof(Stock));
        //            Stocklist.Add(stockResult);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message + " LightspeedVendX ");
        //    }
        //    return Stocklist;
        //}
        public  async Task<List<ProductResponseModel.Product>> GetProducts(string BaseUrl, int StoreId, decimal tax, string ClientId, string ClientSecret, string RefreshToken)
        {
            string Url = "";
            int recordsTotal = 200;
            List<ProductResponseModel.Product> productList = new List<ProductResponseModel.Product>();
            var accesstoken = getaccesstoken(BaseUrl, ClientId, ClientSecret, RefreshToken);
            //var stocks = getStocks(BaseUrl, StoreId, ClientId, ClientSecret, RefreshToken);

            ProductResult ItemList = new ProductResult();
            int page_size = 200;
            int pgcount = 1;
            try
            {
                for (int pageNo = 0; pageNo <= pgcount; pageNo++)
                {
                    string ApiUrl = BaseUrl + "products"+ "?page=" +pageNo+ "&page_size=" +page_size+ "";
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
                        ProductResponseModel.Root prodResult = (ProductResponseModel.Root)JsonConvert.DeserializeObject(content, typeof(ProductResponseModel.Root));
                        var count = prodResult.pagination.results.ToString();
                        var pagesize = prodResult.pagination.page_size.ToString();
                        recordsTotal = Convert.ToInt32(count);
                        page_size = Convert.ToInt32(pagesize);
                        pgcount = prodResult.pagination.pages;
                        productList.AddRange(prodResult.products);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedVendX ");
            }
            return productList;
        }

        public async Task VendhqxLightspeed_Products(int storeid, decimal tax, string BaseUrl, string ClientId, string ClientSecret, string RefreshToken)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            var ProductList = await GetProducts(BaseUrl, storeid, tax, ClientId, ClientSecret, RefreshToken);
            var pList = ProductList;
            //var StockList = getStocks(BaseUrl, storeid, ClientId, ClientSecret, RefreshToken);
            //var sList = StockList.FirstOrDefault().data;
            List<ProductsModel> pf = new List<ProductsModel>();
            List<FullNameModel> fn = new List<FullNameModel>();


            try
            {                
                //var prodList = (from a in pList
                //                select new
                //                {
                //                    storeid = storeid,
                //                    upc = a.sku == null ? "" : a.sku,
                //                    qty = a.inventory == null ? "0" : a.inventory.FirstOrDefault().count,
                //                    sku = a.sku == null ? "" : a.sku,
                //                    pack = 1,
                //                    StoreProductName = a.name,
                //                    StoreDescription = a.name,
                //                    price = a.price,
                //                    sprice = 0,
                //                    start = "",
                //                    end = "",
                //                    tax = tax,
                //                    altupc1 = "",
                //                    altupc2 = "",
                //                    altupc3 = "",
                //                    altupc4 = "",
                //                    altupc5 = "",
                //                    pcat = a.type,
                //                    pcat1 = a.tags
                //                }).Distinct().Select(x => new ProductModel()
                //                {
                //                    StoreID = x.storeid,
                //                    upc = x.upc,
                //                    Qty = Convert.ToInt32(x.qty),
                //                    sku = x.sku,
                //                    pack = 1,
                //                    StoreProductName = x.StoreProductName,
                //                    StoreDescription = x.StoreDescription,
                //                    Price = Convert.ToDecimal(x.price),
                //                    sprice = 0,
                //                    Start = x.start,
                //                    End = x.end,
                //                    Tax = x.tax,
                //                    altupc1 = x.altupc1,
                //                    altupc2 = x.altupc2,
                //                    altupc3 = x.altupc3,
                //                    altupc4 = x.altupc4,
                //                    altupc5 = x.altupc5,
                //                    pcat = x.pcat,
                //                    pcat1 = x.pcat1
                //                }).ToList();

                foreach (var item in ProductList)
                {
                    try
                    {
                        ProductsModel pdf = new ProductsModel();
                        FullNameModel fnf = new FullNameModel();

                        pdf.StoreID = storeid;
                        string upc = "";
                        if (item.sku == "") { continue; }
                        else
                        {
                            upc = item.sku.ToString();
                        }
                        pdf.upc = "#" + upc;
                        fnf.upc = "#" + upc;
                        pdf.sku = "#" + upc;
                        fnf.sku = "#" + upc;
                        pdf.Qty = Convert.ToInt64(item.inventory == null ? "0" : item.inventory.FirstOrDefault().count);
                        pdf.pack = 1;
                        pdf.StoreProductName = item.name.ToString();
                        pdf.StoreDescription = item.name.ToString();
                        if (item.price <= 0) { continue; }
                        else
                        {
                            pdf.Price = Convert.ToDecimal(item.price);
                            fnf.Price = Convert.ToDecimal(item.price);
                        }
                        pdf.sprice = 0;

                        pdf.Tax = tax;

                        pdf.altupc1 = "";
                        pdf.altupc2 = "";
                        pdf.altupc3 = "";
                        pdf.altupc4 = "";
                        pdf.altupc4 = "";
                        pdf.altupc5 = "";
                        fnf.pname = item.name.ToString();
                        fnf.pdesc = item.name.ToString();
                        fnf.pack = 1;
                        fnf.pcat = item.type;
                        fnf.pcat1 = item.tags;
                        fnf.pcat2 = "";
                        fnf.country = "";
                        fnf.region = "";
                        if (!string.IsNullOrEmpty(pdf.upc) && pdf.Price > 0)
                        {
                            pf.Add(pdf);
                            fn.Add(fnf);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
            if (pf.Count > 0 && fn.Count > 0)
            {
                GenerateCSV.GenerateCSVFile(pf, "PRODUCT", storeid, BaseDirectory);
                GenerateCSV.GenerateCSVFile(fn, "FULLNAME", storeid, BaseDirectory);
                Console.WriteLine();
                Console.WriteLine("Product FIle Generated For LightspeedVendX " + storeid);
                Console.WriteLine("Fullname FIle Generated For LightspeedVendX " + storeid);
            }
            else
            {
                Console.WriteLine("Files not generated, No products in the ProductList "+storeid);

            }
        }

    }

    public class category
    {
        public string id { get; set; }
        public string name { get; set; }
        public string deleted_at { get; set; }
        public string version { get; set; }
    }

    public class Type1
    {
        public string id { get; set; }
        public string name { get; set; }
        public string deleted_at { get; set; }
        public string version { get; set; }
    }

    public class Supplier
    {
        public string id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string description { get; set; }
        public string deleted_at { get; set; }
        public string version { get; set; }
    }
    public class Variant_options
    {
        public string id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class ProductResult
    {
        public string id { get; set; }
        public string source_id { get; set; }
        public string handle { get; set; }
        public bool has_variants { get; set; }
        public string variant_parent_id { get; set; }
        public bool active { get; set; }
        public string name { get; set; }
        public decimal price_excluding_tax { get; set; }
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
        public string brand { get; set; }
        public string supplier_name { get; set; }
        public bool track_inventory { get; set; }
        public Type1 type { get; set; }
        public Supplier supplier { get; set; }
        public List<category> categories { get; set; }
        public List<Variant_options> variant_options { get; set; }
    }
    public class Product
    {
        public Pagination pagination { get; set; }
        public List<ProductResult> data { get; set; }
        public string access_token { get; set; }

    }

    public class ProductModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
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
        public string pcat { get; set; }
        public string pcat1 { get; set; }
    }
    public class StockResult
    {
        public string id { get; set; }
        public string outlet_id { get; set; }
        public string product_id { get; set; }
        public int inventory_level { get; set; }
        public int current_amount { get; set; }
        public string version { get; set; }
        public string deleted_at { get; set; }
        public string average_cost { get; set; }
        public string reorder_point { get; set; }
        public string reorder_amount { get; set; }
    }
    public class FullNameModel
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

    public class Stock
    {
        public List<StockResult> data { get; set; }
        public string access_token { get; set; }
    }

    public class Pagination
    {
        public int results { get; set; }
        public int page { get; set; }
        public int page_size { get; set; }
        public int pages { get; set; }
    }


}









