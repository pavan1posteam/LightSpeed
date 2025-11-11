using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedRetail_Api
{
    public class clsLightSppedV3
    {
        private readonly int StoreId;
        private readonly decimal tax;
        private readonly string BaseUrl;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string RefreshToken;
        private readonly int AccountID;
        private readonly bool IsMarkUpPrice;
        private readonly int MarkUpValue;
        public clsLightSppedV3(int _StoreId, decimal _tax, string _BaseUrl, string _ClientId, string _ClientSecret, string _RefreshToken, int _AccountID, bool _IsMarkUpPrice, int _MarkUpValue)
        {
            StoreId = _StoreId;
            tax = _tax;
            BaseUrl = _BaseUrl;
            ClientId = _ClientId;
            ClientSecret = _ClientSecret;
            RefreshToken = _RefreshToken;
            AccountID = _AccountID;
            IsMarkUpPrice = _IsMarkUpPrice;
            MarkUpValue = _MarkUpValue;
        }
        public async Task RunAsync()
        {
            try
            {
                Console.WriteLine("Generating Lightspeed " + StoreId + " Product File....");
                Console.WriteLine("Generating Lightspeed " + StoreId + " Fullname File....");
                await LightSppedV3(StoreId, tax, BaseUrl, ClientId, ClientSecret, RefreshToken, AccountID, IsMarkUpPrice, MarkUpValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
        string MarkupPrice = ConfigurationManager.AppSettings["Markup_Price"];
        string Catbasedupc = ConfigurationManager.AppSettings["cat_based_upc"];
        string consideredupc = ConfigurationManager.AppSettings["considered_upc"];
        string Filters = ConfigurationManager.AppSettings["Filters"];
        string upc_null = ConfigurationManager.AppSettings["upc_null"];
        string catnull = ConfigurationManager.AppSettings["cat_null"];
        string archived = ConfigurationManager.AppSettings["archived"];
        string diffUpc10366 = ConfigurationManager.AppSettings["diffUpc10366"];
        string Upcfilter = ConfigurationManager.AppSettings["Upcfilter"];
        string OtherTax10366 = ConfigurationManager.AppSettings["OtherTax10366"];

        public async Task LightSppedV3(int storeid, decimal tax, string BaseUrl, string ClientId, string ClientSecret, string RefreshToken, int AccountID, bool IsMarkUpPrice, int MarkUpValue)
        {
            List<clsLightProductList.Item> Items = new List<clsLightProductList.Item>();
            try
            {
                var ItemResult = await LightspeedSetting(BaseUrl, storeid, tax, ClientId, ClientSecret, RefreshToken, AccountID);
                List<clsLightProductList.LightProductModel> prodList = new List<clsLightProductList.LightProductModel>();
                List<clsLightProductList.LightFullnameModel> fullNameList = new List<clsLightProductList.LightFullnameModel>();

                foreach (var item in ItemResult)
                {
                    foreach (var record in item)
                    {
                        var abc = (string)record["categoryID"];
                        if (!catnull.Contains(storeid.ToString()))
                        {
                            if (abc == "0")
                            {
                                continue;
                            }
                        }
                        try
                        {
                            clsLightProductList.LightProductModel prod = new clsLightProductList.LightProductModel();
                            clsLightProductList.LightFullnameModel fullName = new clsLightProductList.LightFullnameModel();
                            clsLightProductList.Item ItemList = new clsLightProductList.Item();

                            prod.StoreID = storeid;
                            bool archivedValue = Convert.ToBoolean(record["archived"]);

                            if (archivedValue && archived.Contains(storeid.ToString()))
                            {
                                continue;
                            }
                            var priceItem = JsonConvert.DeserializeObject<clsLightProductList.Prices>(record["Prices"].ToString());
                            var json2 = JsonConvert.SerializeObject(record["Prices"]);
                            var prc = priceItem.ItemPrice.ToString();
                            prc = prc.ToString().Split(':', ',')[1];
                            prc = prc.Replace("\"", "");

                            string pr = prc;
                            prod.Price = Convert.ToDecimal(prc);
                            fullName.Price = Convert.ToDecimal(prc);
                            if (MarkupPrice.Contains(storeid.ToString()))     //11370
                            {
                                if (IsMarkUpPrice)
                                {
                                    decimal markup = Convert.ToDecimal(prc) - (Convert.ToDecimal(prc) * MarkUpValue / 100);
                                    prod.Price = (markup);
                                    fullName.Price = (markup);
                                }
                            }
                            var qtyItem = JsonConvert.DeserializeObject<clsLightProductList.ItemShops>(record["ItemShops"].ToString());
                            var json = JsonConvert.SerializeObject(record["ItemShops"]);
                            var qtyy = qtyItem.ItemShop.ToString();
                            qtyy = qtyy.ToString().Split(':', ',')[3];
                            qtyy = qtyy.Replace("\"", "");

                            string qt = qtyy;
                            prod.Qty = Convert.ToInt64(qt) > 0 ? Convert.ToInt64(qt) : 0;

                            var Cat_Item = !catnull.Contains(storeid.ToString()) ? JsonConvert.DeserializeObject<clsLightProductList.Category>(record["Category"].ToString()) : null;
                            var json_cat = !catnull.Contains(storeid.ToString()) ? JsonConvert.SerializeObject(record["Category"]) : null;

                            string upc = (string)record["upc"];
                            string cat = !catnull.Contains(storeid.ToString()) ? Cat_Item.name.ToString() : null;
                            if (Catbasedupc.Contains(storeid.ToString()))       //10936                                                                            ////31-05-2021(tckt#9597)
                            {
                                if (string.IsNullOrEmpty(upc) && (cat == "CIGARS" || cat == "CIGAR" || cat == "CIGARETTES"))
                                {
                                    prod.upc = '#' + (string)record["customSku"];
                                    fullName.upc = '#' + (string)record["customSku"];
                                }

                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }
                            else if (consideredupc.Contains(storeid.ToString()))      //11312
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    prod.upc = '#' + (string)record["manufacturerSku"];
                                    fullName.upc = '#' + (string)record["manufacturerSku"];
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }

                            }
                            else if (upc_null.Contains(storeid.ToString()))
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    prod.upc = (string)record["ean"] == "" ? '#' + (string)record["customSku"] : '#' + (string)record["ean"];
                                    fullName.upc = (string)record["ean"] == "" ? '#' + (string)record["customSku"] : '#' + (string)record["ean"];
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }
                            else if (catnull.Contains(storeid.ToString()))
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    prod.upc = '#' + (string)record["customSku"];
                                    fullName.upc = '#' + (string)record["customSku"];
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }
                            else if (diffUpc10366.Contains(storeid.ToString()))
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    prod.upc = "#" + (string)record["systemSku"];
                                    fullName.upc = "#" + (string)record["systemSku"];
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }
                            else if (Upcfilter.Contains(storeid.ToString()))
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    continue;
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"].ToString().TrimStart('0');
                                    fullName.upc = '#' + (string)record["upc"].ToString().TrimStart('0');
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    continue;
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }
                            string sku = (string)record["itemID"];
                            prod.sku = '#' + sku;
                            fullName.sku = '#' + sku;
                            prod.pack = 1;
                            fullName.pack = 1;
                            prod.StoreProductName = (string)record["description"];
                            prod.StoreDescription = (string)record["description"];
                            prod.sprice = 0;
                            prod.tax = tax;
                            prod.Start = "";
                            prod.End = "";
                            prod.altupc1 = "";
                            prod.altupc2 = "";
                            prod.altupc3 = "";
                            prod.altupc4 = "";
                            prod.altupc5 = "";
                            fullName.pname = (string)record["description"];
                            fullName.pdesc = (string)record["description"];
                            fullName.pcat = !catnull.Contains(storeid.ToString()) ? Cat_Item.fullPathName.ToString() : null;
                            if (OtherTax10366.Contains(storeid.ToString()) && fullName.pcat == "CBD Product" || fullName.pcat == "CBD Seltzer" || fullName.pcat == "CBD")
                            {
                                prod.tax = 0.1245M;
                            }
                            fullName.pcat1 = "";
                            fullName.pcat2 = "";
                            prod.uom = "";
                            fullName.uom = "";
                            fullName.country = "";
                            fullName.region = "";
                            if (Filters.Contains(storeid.ToString()))    //10932
                            {
                                if (prod.Price > 0 && prod.upc.Trim() != "#" && prod.Qty > 0)
                                {
                                    prodList.Add(prod);
                                    fullNameList.Add(fullName);
                                }
                            }

                            else if (Upcfilter.Contains(storeid.ToString()))
                            {
                                if (prod.Price > 0 && prod.Qty > 0 && prod.upc.Trim() != "#" && prod.StoreDescription != "SHOW CIGARILLOS TROPICAL TWISTA" && prod.StoreDescription != "SHOW CIGARILLOS SWEET" && prod.StoreDescription != "SHOW CIGARILLOS WHITE GRAPE" && prod.StoreDescription != "SHOW CIGARILLOS MANGO" && prod.StoreDescription != "KUSH BLACK 2 TIPPED CIGARS" && prod.StoreDescription != "PRIME TIME LITTLE CIGARS")
                                {
                                    prodList.Add(prod);
                                    fullNameList.Add(fullName);
                                }
                            }
                            else
                            {
                                if (prod.Price > 0 && prod.upc.Trim() != "#" && prod.StoreDescription != "SHOW CIGARILLOS TROPICAL TWISTA" && prod.StoreDescription != "SHOW CIGARILLOS SWEET" && prod.StoreDescription != "SHOW CIGARILLOS WHITE GRAPE" && prod.StoreDescription != "SHOW CIGARILLOS MANGO" && prod.StoreDescription != "KUSH BLACK 2 TIPPED CIGARS" && prod.StoreDescription != "PRIME TIME LITTLE CIGARS")
                                {
                                    prodList.Add(prod);
                                    fullNameList.Add(fullName);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                if (prodList.Count > 0 && fullNameList.Count > 0)
                {
                    GenerateCSV.GenerateCSVFile(prodList, "PRODUCT", storeid, BaseDirectory);
                    if (!catnull.Contains(storeid.ToString()))
                    {
                        GenerateCSV.GenerateCSVFile(fullNameList, "FULLNAME", storeid, BaseDirectory);
                    }
                    Console.WriteLine("Product FIle Generated For LightspeedRetailPos " + storeid);
                    if (!catnull.Contains(storeid.ToString()))
                    {
                        Console.WriteLine("Fullname FIle Generated For LightspeedRetailPos " + storeid);
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Files not generated, No products in the ProductList " + storeid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in LIghtspeed @" + storeid + DateTime.UtcNow + " GMT", " ERROR  In Response while Generating the Filess " + ":" + ex.Message + "<br/>" + ex.StackTrace);

            }
        }

        public async Task<string> getProduct(string BaseUrl, int storeid, string clientid, string clientsecret, string RefreshToken)
        {
            string AccessToken = "";
            clsLightProductList.Root prod = new clsLightProductList.Root();
            try
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest("", Method.Post);
                //request.AddHeader("content-type", "multipart/form-data");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded"); 
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("client_id", clientid);
                request.AddParameter("client_secret", clientsecret);
                request.AddParameter("refresh_token", RefreshToken);
                request.AddParameter("grant_type", "refresh_token");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var result = JsonConvert.DeserializeObject<clsLightProductList.Root>(content);
                    AccessToken = result.access_token;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedRetailAPI ");
                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in " + " Lightspeed Pos@" + DateTime.UtcNow + " GMT", " ERROR  In Response while getting Access_token " + ":" + ex.Message + "<br/>" + ex.StackTrace);
            }
            return AccessToken;
        }

        public async Task<List<JArray>> LightspeedSetting(string BaseUrl, int StoreId, decimal tax, string ClientId, string ClientSecret, string RefreshToken, int AccountID)
        {
            string Url = "";
            clsLightProductList.Result obj = new clsLightProductList.Result();
            //int recordsTotal = 100;
            List<JArray> productList = new List<JArray>();
            var accesstoken = getProduct(BaseUrl, StoreId, ClientId, ClientSecret, RefreshToken);
            BaseUrl = "https://api.lightspeedapp.com/API/V3/Account/" + AccountID + "/";
            clsLightProductList.Root prod = new clsLightProductList.Root();
            //string shops = "load_relations=[\"ItemShops\",\"Category\"]";

            try
            {
                bool isFirstLoop = true;
                while (isFirstLoop || !string.IsNullOrEmpty(Url)) // Loop until Url is null or empty
                {
                    string ApiUrl = isFirstLoop ? BaseUrl + "Item.json?load_relations=[\"ItemShops\",\"Category\"]" : Url;
                    isFirstLoop = false;
                    ApiUrl = string.IsNullOrEmpty(Url) ? ApiUrl : Url;
                    var client = new RestClient(ApiUrl);
                    var request = new RestRequest("", Method.Get);
                    request.AddHeader("Authorization", "Bearer " + accesstoken.Result);
                    request.AddHeader("cache-control", "no-cache");
                    //request.AddHeader("content-type", "application/json");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var response = await client.ExecuteAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = response.Content;
                        var jsonResponse = JObject.Parse(content);
                        var attributes = jsonResponse["@attributes"];
                        string nextUrl = attributes?["next"]?.ToString();
                        var jArray = (JArray)jsonResponse["Item"];
                        if (jArray == null || !jArray.Any())
                        {
                            break; 
                        }
                        productList.Add(jArray);

                        Url = nextUrl;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedRetailAPI ");
                (new clsEmail()).sendEmail(
                    DeveloperId, "", "",
                    "Error in " + " Lightspeed Pos@" + DateTime.UtcNow + " GMT",
                    " ERROR  In Response while getting Product Pagination " + ":" + ex.Message + "<br/>" + ex.StackTrace
                );
            }

            return productList;
        }

        public class clsLightProductList
        {
            public class Root
            {
                public object @attributes { get; set; }
                public List<Item> Items { get; set; }
                public string access_token { get; set; }
            }
            public class Item
            {
                public object @attributes { get; set; }
                public int itemID { get; set; }
                public int systemSku { get; set; }
                public float defaultCost { get; set; }
                public float avgCost { get; set; }
                public Boolean discountable { get; set; }
                public Boolean archived { get; set; }
                public int itemType { get; set; }
                public Boolean serialized { get; set; }
                public string description { get; set; }
                public int modelYear { get; set; }
                public string upc { get; set; }
                public string ean { get; set; }
                public string customSku { get; set; }
                public string manufacturerSku { get; set; }
                public DateTime createTime { get; set; }
                public DateTime timeStamp { get; set; }
                public Boolean publishToEcom { get; set; }
                public int categoryID { get; set; }
                public int taxClassID { get; set; }
                public int departmentID { get; set; }
                public int itemMatrixID { get; set; }
                public int manufacturerID { get; set; }
                public int seasonID { get; set; }
                public int defaultVendorID { get; set; }
                public object ItemShops { get; set; }
                public object Prices { get; set; }
                public object category { get; set; }
                public int catvID { get; set; }
                public string catname { get; set; }
            }
            public class ItemList
            {
                [JsonProperty("@attributes")]
                public attributes attributes { get; set; }
                [JsonProperty("Item")]
                public List<Item> items { get; set; }
            }
            public class ItemShops
            {
                public object ItemShop { get; set; }
                public int itemShopID { get; set; }
                public int qoh { get; set; }
                public int sellable { get; set; }
                public int backorder { get; set; }
                public int componentQoh { get; set; }
                public int componentBackorder { get; set; }
                public int reorderPoint { get; set; }
                public int reorderLevel { get; set; }
                public DateTime timeStamp { get; set; }
                public int itemID { get; set; }
                public int shopID { get; set; }
            }
            public class ItemShop
            {
                // public object ItemShop { get; set; }
                public int itemShopID { get; set; }
                public int qoh { get; set; }
                public int sellable { get; set; }
                public int backorder { get; set; }
                public int componentQoh { get; set; }
                public int componentBackorder { get; set; }
                public int reorderPoint { get; set; }
                public int reorderLevel { get; set; }
                public DateTime timeStamp { get; set; }
                public int itemID { get; set; }
                public int shopID { get; set; }
            }
            public class Prices
            {
                public object ItemPrice { get; set; }
                // public decimal amount { get; set; }
                public string useTypeID { get; set; }
                public string useType { get; set; }
            }
            public class ItemPrice
            {
                // public object ItemPrice { get; set; }
                public decimal amount { get; set; }
                public string useTypeID { get; set; }
                public string useType { get; set; }
            }
            public class Result
            {
                public string Response { get; set; }
                public string Url { get; set; }
            }
            public class @attributes
            {
                //public string @attributes { get; set; }
                public int count { get; set; }
                public List<Item> Items { get; set; }

                public string offset { get; set; }
                public string limit { get; set; }
            }
            public class Category
            {
                public int categoryID { get; set; }
                public string name { get; set; }
                public int nodeDepth { get; set; }
                public string fullPathName { get; set; }
                public int leftNode { get; set; }
                public int rightNode { get; set; }
                public int parentID { get; set; }
                public DateTime createTime { get; set; }
                public DateTime timeStamp { get; set; }
            }

            public class LightFullnameModel
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
            public class LightProductModel
            {
                public int StoreID { get; set; }
                public string upc { get; set; }
                public Int64 Qty { get; set; }
                public string sku { get; set; }
                public int pack { get; set; }
                public string uom { get; set; }
                public string StoreProductName { get; set; }
                public string StoreDescription { get; set; }
                public decimal Price { get; set; }
                public decimal sprice { get; set; }
                public string Start { get; set; }
                public string End { get; set; }
                public decimal tax { get; set; }
                public string altupc1 { get; set; }
                public string altupc2 { get; set; }
                public string altupc3 { get; set; }
                public string altupc4 { get; set; }
                public string altupc5 { get; set; }
                // public int catID { get; set; }
            }
        }
    }
}
