using System;
using System.Configuration;


namespace LightspeedRetail_Api
{
    class Program
    {
        private static void Main(string[] args)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string lightspeedAPIkey = ConfigurationManager.AppSettings["lightspeedAPIkey"];
            string lightspeedRSeries = ConfigurationManager.AppSettings["lightspeedRSeries"];


            try
            {
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                foreach (POSSetting posDetail in pOSSettings.PosDetails)
                {
                    try
                    {
                        if (posDetail.PosName.ToUpper() == "LIGHTSPEED")
                        {

                            if (posDetail.StoreSettings.StoreId == 10716 || posDetail.StoreSettings.StoreId == 10717 || posDetail.StoreSettings.StoreId == 11267)
                            {
                                clsLightspeedAPI clsLightspeedAPI = new clsLightspeedAPI(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.APIKey);
                                clsLightspeedAPI.RunAsync().GetAwaiter().GetResult();
                                Console.WriteLine();
                            }
                            else if (lightspeedRSeries.Contains(posDetail.StoreSettings.StoreId.ToString()))
                            {
                                clsLightSpeedRSeries rSeries = new clsLightSpeedRSeries(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.AccountID, posDetail.Refresh_token);
                                rSeries.RunAsync().GetAwaiter().GetResult();
                                Console.WriteLine();
                            }
                            else if (posDetail.StoreSettings.StoreId != 20010 && posDetail.StoreSettings.StoreId != 20011)
                            {
                                if (posDetail.StoreSettings.StoreId == 11475) // this is only for the store 11475
                                {
                                    VendhqxLightspeed clsLightspeed_vendX = new VendhqxLightspeed(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.RefreshToken);
                                    clsLightspeed_vendX.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else if (posDetail.StoreSettings.StoreId == 11917) // this is only for the store 11917
                                {
                                    clsLightspeedXAPI lightspeedX = new clsLightspeedXAPI(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.RefreshToken);
                                    lightspeedX.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else if (posDetail.StoreSettings.StoreId == 12097)// As per ticket #47897
                                {
                                    int[] hour = { 0, 4, 8, 12, 16, 20, 24};
                                    int currentHour = DateTime.UtcNow.Hour;
                                    for(int i = 0; i < hour.Length; i++)
                                    {
                                        if(hour[i] == currentHour)
                                        {
                                            clsLightspeedAPI_X lightspeedAPI_X = new clsLightspeedAPI_X(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.APIKey);
                                            lightspeedAPI_X.RunAsync().GetAwaiter().GetResult();
                                            Console.WriteLine();
                                        }
                                    }
                                }
                                else if (lightspeedAPIkey.Contains(posDetail.StoreSettings.StoreId.ToString()))  // X Series  // this is only for the stores 12160 , 12233
                                {
                                    clsLightspeedAPI_X lightspeedAPI_X = new clsLightspeedAPI_X(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.APIKey);
                                    lightspeedAPI_X.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else if (posDetail.StoreSettings.StoreId == 10366)  // 10366
                                {
                                    clsLightSppedV3 clsLightSppedV3 = new clsLightSppedV3(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.RefreshToken, posDetail.StoreSettings.POSSettings.AccountID, posDetail.StoreSettings.POSSettings.IsMarkUpPrice, posDetail.StoreSettings.POSSettings.MarkUpValue);
                                    clsLightSppedV3.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else if (posDetail.StoreSettings.StoreId == 12187)
                                {
                                    clsLighspeedRetailV3 clsLighspeedRetailV3 = new clsLighspeedRetailV3(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.RefreshToken, posDetail.StoreSettings.POSSettings.AccountID, posDetail.StoreSettings.POSSettings.IsMarkUpPrice, posDetail.StoreSettings.POSSettings.MarkUpValue);
                                    clsLighspeedRetailV3.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else if (!string.IsNullOrEmpty(posDetail.StoreSettings.POSSettings.APIKey))
                                {
                                    clsLightspeedAPI_X lightspeedAPI_X = new clsLightspeedAPI_X(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.APIKey);
                                    lightspeedAPI_X.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else if (!string.IsNullOrEmpty(posDetail.Refresh_token))
                                {
                                    clsLightSpeedRSeries rSeries = new clsLightSpeedRSeries(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.AccountID, posDetail.Refresh_token);
                                    rSeries.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                                else// remaining all stores
                                {
                                    clsLightspeedRetail_Api clsLightspeedRetail_Api = new clsLightspeedRetail_Api(posDetail.StoreSettings.StoreId, posDetail.StoreSettings.POSSettings.tax, posDetail.StoreSettings.POSSettings.BaseUrl, posDetail.StoreSettings.POSSettings.ClientId, posDetail.StoreSettings.POSSettings.ClientSecret, posDetail.StoreSettings.POSSettings.RefreshToken, posDetail.StoreSettings.POSSettings.AccountID, posDetail.StoreSettings.POSSettings.IsMarkUpPrice, posDetail.StoreSettings.POSSettings.MarkUpValue);
                                    clsLightspeedRetail_Api.RunAsync().GetAwaiter().GetResult();
                                    Console.WriteLine();
                                }
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                    }
                }
            }

            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in LightspeedRetail_Api @" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
        }
    }
}
