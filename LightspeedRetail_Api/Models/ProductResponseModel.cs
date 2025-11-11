using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedRetail_Api.Models
{
    class ProductResponseModel
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Image
        {
            public string id { get; set; }
            public Links links { get; set; }
        }

        public class Inventory
        {
            public string outlet_id { get; set; }
            public string outlet_name { get; set; }
            public string count { get; set; }
            public string reorder_point { get; set; }
            public string restock_level { get; set; }
        }

        public class Links
        {
            public string original { get; set; }
            public string standard { get; set; }
            public string thumb { get; set; }
        }

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
            public List<Image> images { get; set; }
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
            public string type { get; set; }
            public List<Taxis> taxes { get; set; }
            public string supplier_name { get; set; }
            public string variant_source_id { get; set; }
            public string base_name { get; set; }
            public string brand_name { get; set; }
            public bool track_inventory { get; set; }
            public string brand_id { get; set; }
            public List<Inventory> inventory { get; set; }
        }

        public class Root
        {
            public Pagination pagination { get; set; }
            public List<Product> products { get; set; }
        }

        public class Taxis
        {
            public string outlet_id { get; set; }
            public string tax_id { get; set; }
        }


    }
}
