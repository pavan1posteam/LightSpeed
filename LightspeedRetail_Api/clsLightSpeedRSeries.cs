using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static LightspeedRetail_Api.clsLighspeedRetailV3;
using static LightspeedRetail_Api.clsLightSppedV3.clsLightProductList;

namespace LightspeedRetail_Api
{
    class clsLightSpeedRSeries
    {
        string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string showonline = ConfigurationManager.AppSettings["showonline"];
        private readonly int StoreId;
        private readonly decimal tax;
        private readonly string BaseUrl;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string RefreshToken;
        private readonly int AccountID;
        public clsLightSpeedRSeries(int _StoreId, decimal _tax, string _BaseUrl, string _ClientId, string _ClientSecret, int _AccountID, string _RefreshToken)
        {
            StoreId = _StoreId;
            tax = _tax;
            BaseUrl = _BaseUrl;
            ClientId = _ClientId;
            ClientSecret = _ClientSecret;
            RefreshToken = _RefreshToken;
            AccountID = _AccountID;
            Console.WriteLine("Generating Lightspeed " + StoreId + " Product File....");
            Console.WriteLine("Generating Lightspeed " + StoreId + " Fullname File....");
        }
        public async Task RunAsync()
        {
            try
            {
                string[] array = LG_RefreshToken(BaseUrl, ClientId, ClientSecret, RefreshToken);
                List<ClsLightProductList.Item> item = await LightspeedSetting(BaseUrl, ClientId, ClientSecret, array[0], AccountID);

                //For only Testing, Comment this
                //string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IlJUX09BVVRIMl8yMDI0XzA3XzIyIn0.eyJqdGkiOiJiMDJhYzgzYmQ4N2YxZDQxMWM5YzIxNmE4OWZmOThlZGIwMDI4NWE3YjJlYjU4ZThhZTRiZWQzZDZlOTU2YmExOGRhYTgyNjg0MmVjY2YxNiIsImlzcyI6Imh0dHBzOi8vY2xvdWQubGlnaHRzcGVlZGFwcC5jb20iLCJhdWQiOiJhOTdiYjI4YWMxY2EyN2Q3YjkzNGI2ZmU1ZTYwMThhMzcwZTM4OGEwZmU5ZGJhZjljNDI5MWFkOTZiNjE3M2JjIiwic3ViIjoiMTI2MzE5MCIsImFjY3QiOiIxODk5NzQiLCJzY29wZSI6ImVtcGxveWVlOmFkbWluIiwiaWF0IjoxNzUzMjUyODI2LjUxNzA2MiwibmJmIjoxNzUzMjUyODI2LjUxNzA2MiwiZXhwIjoxNzUzMjU2NDI2LjQ5MjUyNn0.MIM5L57WjBdIIxYjA52wUNNVU1txPk5y_R2fzdFD-CDe00LwY4Ai0ddRKgIQyfE7btMd2zSxwE0mExUQRF6jQVjZLE-G_YRNjZz3do19boXhasl2wdZyjGKjlF2H7ugX0twue7eTsdkslnS-srh29N6u1cVcpZTqV3SxdkIUp0JUEASXGNdrBddBUe0RsR_i1UX3hiIBLSJ4usjSEtYzH5xmOZJ9F5241fYuNNva3c2oGra3r15fxpEXFp68OzVdQKuF-YM6eZd6uTnwGRmtuKG4nOkr_RzXZ1By5g9MvsiEDxTV8VmaZxutEY2NZ3qIqF4QnxqV-0ownu6khRIzxQ";
                //List<ClsLightProductList.Item> item = await LightspeedSetting(BaseUrl, ClientId, ClientSecret, token, AccountID);


                Parsing(item, StoreId, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public string[] LG_RefreshToken(string BaseUrl, string ClientId, string ClientSecret, string refreshtoken)
        {
            string[] token_info = new string[2];
            string accessToken = "";
            BaseUrl = "https://cloud.lightspeedapp.com/auth/oauth/token";
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("");
            request.AddHeader("Content-Type", "application/json");
            var body = new
            {
                client_id = ClientId,
                client_secret = ClientSecret,
                grant_type = "refresh_token",
                //refresh_token = refreshtoken
                refresh_token = "def50200d68e254b851765352e78c214ec35dd905ebd5c731b974391417ebd227257a5a4b6eebb10c844a76602f03a58a2436456522a40cef6ae972c1e85a133af971051b52042ec91ec1c543f65d67332e0acc33587b2e0fdcc4d0768e0b36ad8a377c79785c9bf2232ebe2419cefe94d7d379fda91c8db0c82e0d242970dd1e467b7c9af8d4842ec613cab9562cec655eb17b6737b7b9541fb020d322bd547aecc46dcb170097d44a95426dd44ab4987832b870afb77c860986e9a13add7f9650cbf95f45ec1bce6a16a2b9421295091ceea20ce537e7ef9099318096d865561e072825c13d4d623b875d1b6cb03db8388831bfc0958d663629e546bda9facbe373282eb79fc4de238a889862f92ac195d5e0507896bfb7d09bfa989a34c47cca808a852612be960efce71b6044d0b1cee9af1f277184c0fa733aab962bf7b483667a88af3787586001e465c754062b6d60cd6c5960fbf5fcaa2c827ebe780f4bf9da220e23cc92f826e096383f3dda7978f59c5050f534704858645a6b952f6ca47b5b81e51b210fd58bddf175dedf30dac6a21b44a198b8c1e36db3b0db7a828992c9c5fc3459ecae024ad0bbb471e0118a1a4a88c84"

            };
            string jsonBody = JsonConvert.SerializeObject(body);
            request.AddStringBody(jsonBody, DataFormat.Json);
            string requestJson = JsonConvert.SerializeObject(body);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var response = client.Execute(request, Method.Post);
            string responseContent = response.Content;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic data = JsonConvert.DeserializeObject(responseContent);
                accessToken = data.access_token;
                refreshtoken = data.refresh_token;

                token_info[0] = accessToken;
                token_info[1] = refreshtoken;

                try
                {
                    List<SqlParameter> parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@StoreId", StoreId),
                        new SqlParameter("@AccessToken", accessToken),
                        new SqlParameter("@refresh_token", refreshtoken)
                    };

                    DatabaseObject db = new DatabaseObject();
                    db.GetDataTable("usp_bc_LightSpeedAccessTokenInsert", parameters);
                }
                catch { }
            }
            return token_info;
        }

        public async Task<List<ClsLightProductList.Item>> LightspeedSetting(string BaseUrl, string ClientId, string ClientSecret, string accesstoken, int AccountID)
        {
            string Url = "";
            int recordsTotal = 100;
            List<ClsLightProductList.Item> allItems = new List<ClsLightProductList.Item>();

            if (!string.IsNullOrEmpty(accesstoken))
            {
                BaseUrl = "https://api.lightspeedapp.com/API/Account/" + AccountID + "/";

                try
                {
                    for (int pageNo = 0; pageNo <= recordsTotal - 100; pageNo++)
                    {
                        string shops = "load_relations=[\"ItemShops\",\"Category\"]";
                        string ApiUrl = BaseUrl + "Item.json" + "?" + shops;
                        ApiUrl = string.IsNullOrEmpty(Url) ? ApiUrl : Url;

                        var client = new RestClient(ApiUrl);
                        var request = new RestRequest("");
                        request.AddHeader("Authorization", "Bearer " + accesstoken);
                        request.AddHeader("cache-control", "no-cache");
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        var response = await client.ExecuteAsync(request, Method.Get);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var itemList = JsonConvert.DeserializeObject<ClsLightProductList.ItemList>(response.Content);
                            if (itemList != null && itemList.items != null)
                            {
                                allItems.AddRange(itemList.items);

                                string lastItemId = itemList.items.LastOrDefault()?.itemID.ToString();
                                if (!string.IsNullOrEmpty(lastItemId))
                                {
                                    Url = BaseUrl + $"Item.json?orderby=itemID&itemID=%3E%2C{lastItemId}&" + shops;
                                }
                                recordsTotal = Convert.ToInt32(itemList.attributes.count);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error fetching data: " + response.StatusCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " LightspeedRetailAPI ");
                }

                return allItems;
            }
            else
            {
                Console.WriteLine("Refresh Token Expired", StoreId);
            }

            return new List<ClsLightProductList.Item>(); 
        }
        public void Parsing(List<ClsLightProductList.Item> ItemResult, int storeid, decimal tax)
        {
            List<ClsLightProductList.LightProductModel> prodList = new List<ClsLightProductList.LightProductModel>();
            List<ClsLightProductList.LightFullnameModel> fullNameList = new List<ClsLightProductList.LightFullnameModel>();
            if (ItemResult.Count > 0)
            {
                foreach (var data in ItemResult)
                {
                    ClsLightProductList.LightProductModel prod = new ClsLightProductList.LightProductModel();
                    ClsLightProductList.LightFullnameModel fullName = new ClsLightProductList.LightFullnameModel();
                    if (showonline.Contains(storeid.ToString()) && !data.publishToEcom)
                    {
                        continue;
                    }
                    prod.StoreID = storeid;
                    if (string.IsNullOrEmpty(data.upc))
                        prod.upc = "#" + data.systemSku;
                    else
                        prod.upc = "#" + data.upc;
                    fullName.upc = prod.upc;

                    prod.sku = "#" + data.systemSku;
                    fullName.sku = prod.sku;
                    prod.Qty = data.ItemShops?.ItemShop?.FirstOrDefault() != null ? Convert.ToInt32(data.ItemShops.ItemShop.First().qoh) : 0;
                    prod.pack = 1;
                    fullName.pack = prod.pack;
                    prod.uom = "";
                    fullName.uom = prod.uom;
                    prod.StoreProductName = data.description;
                    prod.StoreDescription = prod.StoreProductName;
                    fullName.pname = prod.StoreProductName;
                    fullName.pdesc = prod.StoreProductName;
                    prod.Price = Convert.ToDecimal(data.Prices.ItemPrice[0].amount);
                    prod.sprice = 0;
                    fullName.Price = prod.Price;
                    prod.tax = tax;
                    if (!string.IsNullOrEmpty(data.category?.fullPathName))
                    {
                        var categories = data.category.fullPathName.Split('/');

                        fullName.pcat = categories.Length > 0 ? categories[0] : "";
                        fullName.pcat1 = categories.Length > 1 ? categories[1] : "";
                        fullName.pcat2 = categories.Length > 2 ? categories[2] : "";
                    }
                    else
                    {
                        fullName.pcat = fullName.pcat1 = fullName.pcat2 = "";
                    }
                    if (prod.Price > 0 && prod.upc.Length > 2)
                    {
                        prodList.Add(prod);
                        fullNameList.Add(fullName);
                    }

                }
                if (prodList.Count > 0 && fullNameList.Count > 0)
                {
                    DataTable dtproduct = ToDataTable(prodList);
                    DataTable dtfullname = ToDataTable(fullNameList);
                    Console.WriteLine("Generating CSV Files");
                    string product = GenerateCSV.GenerateCSVFile(dtproduct, "PRODUCT", storeid, BaseDirectory);
                    string fullname = GenerateCSV.GenerateCSVFile(dtfullname, "FULLNAME", storeid, BaseDirectory);
                    Console.WriteLine("Product FIle Generated For LightspeedRetailPos " + storeid);
                    Console.WriteLine("Fullname FIle Generated For LightspeedRetailPos " + storeid);
                }
                else
                {
                    Console.WriteLine("Files not generated, No products in the ProductList " + storeid);
                }
            }
            else
            {
                Console.WriteLine("No Products Found");
            }
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable table = new DataTable(typeof(T).Name);
            var propList = typeof(T).GetProperties();

            foreach (var prop in propList)
            {
                Type colType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                table.Columns.Add(prop.Name, colType);
            }

            foreach (var item in items)
            {
                var values = new object[propList.Length];
                for (int i = 0; i < propList.Length; i++)
                {
                    values[i] = propList[i].GetValue(item, null);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
    public class ClsLightProductList
    {
        internal class Root
        {
            public object @attributes { get; set; }
            public List<Item> Items { get; set; }
            public string access_token { get; set; }
        }
        public class Item
        {
            public int itemID { get; set; }
            public string systemSku { get; set; }
            public float defaultCost { get; set; }
            public float avgCost { get; set; }
            public bool discountable { get; set; }
            public bool archived { get; set; }
            public string itemType { get; set; }
            public bool serialized { get; set; }
            public string description { get; set; }
            public int modelYear { get; set; }
            public string upc { get; set; }
            public string ean { get; set; }
            public string customSku { get; set; }
            public string manufacturerSku { get; set; }
            public DateTime createTime { get; set; }
            public DateTime timeStamp { get; set; }
            public bool publishToEcom { get; set; }
            public int categoryID { get; set; }
            public int taxClassID { get; set; }
            public int departmentID { get; set; }
            public int itemMatrixID { get; set; }
            public int manufacturerID { get; set; }
            public int seasonID { get; set; }
            public int defaultVendorID { get; set; }
            public ItemShops ItemShops { get; set; }
            public Prices Prices { get; set; }
            public Category category { get; set; }
            public int catvID { get; set; }
            public string catname { get; set; }
        }
        public class ItemShops
        {
            public List<ItemShop> ItemShop { get; set; }
            public int itemShopID { get; set; }
            public string qoh { get; set; }
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
            //public object ItemShop { get; set; }
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
            public List<ItemPrice> ItemPrice { get; set; }
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
        public class attributes
        {
            //public string @attributes { get; set; }
            public int count { get; set; }

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
            public decimal tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
            // public int catID { get; set; }
        }

        public class Refresh
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }
        public class ItemList
        {
            [JsonProperty("@attributes")]
            public attributes attributes { get; set; }
            [JsonProperty("Item")]
            public List<Item> items { get; set; }
        }
        public class Item1
        {
            public string systemSku { get; set; }
            public string description { get; set; }
            public string upc { get; set; }
            public string discountable { get; set; }

        }
    }
}
