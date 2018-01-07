using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Intranet.Data.EF;
using Intranet.Messages;
using Intranet.Models;
using Intranet.Models.Enums;
using Utilities;

namespace Intranet.Business
{
    //todo jay - add the actual emailing part to messaging infrastructure
    public static class UoWTonerManager
    {
        
        //todo -jay: store these in a config Db table
        private static readonly string FromEmail = WebConfigurationManager.AppSettings["TonerFromEmail"];
        private static readonly string CcAddress = WebConfigurationManager.AppSettings["TonerCcAddress"];
        private static readonly string TellNumber = WebConfigurationManager.AppSettings["NfbTellNumber"];

        private static string BuildToner(List<PrinterPropertyModel> printerProperties)
        {
            return null;
        }

        //this is to make sure all related printer properties are loaded
        private static List<PrinterPropertyModel> RebuildProperties(List<TonerOrderDetailsModel> orders)
        {
            using (var store = new DataContextEF())
            {
                var tempList = new List<PrinterPropertyModel>();
                var recStatus = RecordStatusEnum.Active.ToString();

                orders.ForEach((p) =>
                    {
                        var data =
                            store.PrinterProperties.Where(
                                m => m.RecordStatus.Equals(recStatus) && m.PropertyId.Equals(p.PropertyId));

                        if (data.Any())
                            tempList.Add(data.FirstOrDefault());
                    });

                return tempList.ToList();
            }

        }

        public static bool OrderToner(PrinterModel printer, string staffName, List<TonerOrderDetailsModel> orderDetails)
        {
            if (!orderDetails.Any())
                return false;

            var orders = RebuildProperties(orderDetails);

            try
            {
                var client = new SmtpClient();

                //todo - jay: get email type from the db
                var body = new StringBuilder();

                body.Append("Dear Supplier");
                body.Append("\n This is an automated system requesting toner for the following printer please:");
                body.Append("\n Serial No\t: " + printer.SerialNumber);
                body.Append("\n Cartridge Colours:");

                orders.ForEach(m => body.Append(String.Format("\t{0}\n", m.PropertyDescription)));

                body.Append("\n Should you have any queries, please use the contact details below");
                body.Append("\n");
                body.Append("\n Kind regards");
                body.Append("\n NVestholdings Information Technology Division");
                body.Append(String.Format("\n Email \t: {0}", CcAddress));
                body.Append(String.Format("\n Tel\t:{0}", TellNumber));

                var mailMessenger = new MailMessage(new MailAddress(FromEmail),
                                                    new MailAddress(printer.PrinterProvider.ProviderEmail))
                    {
                        Subject =
                            String.Format("Toner order date {0} from {1}", DateTime.Now.ToString("dd/MM/yyy"), staffName),
                        Body = body.ToString()
                    };

                client.Send(mailMessenger);

                return true;
            }
            catch (SmtpException exception)
            {
                //log 4 net will come in handy on here

                return false;
            }
            catch (Exception exception)
            {
                //log 4 net will come in handy on here

                return false;
            }
        }

        public static bool ClearOldTonerOrders()
        {
            using (var contextEf = new DataContextEF())
            {
                var openTonerOrders = contextEf.TonerOrderDetails.Where(x => x.OrderStatus == (int)OrderStatus.Opened).ToList();
                if (!openTonerOrders.Any())
                {
                    return false;
                }
                else if(openTonerOrders.Any())
                {
                    for (int i = 0; i < openTonerOrders.Count; i++)
                    {
                        TonerOrderDetailsModel order = openTonerOrders[i];
                        var data = contextEf.TonerOrders.FirstOrDefault(x => x.OrderId.Equals(order.OrderId));
                        if (data != null)
                        {
                            TonerOrdersModel ordersModel = data;
                            openTonerOrders[i].TonerOrder = ordersModel;
                        }
                        if (order.TonerOrder.OrderDate.Date.AddDays(2) > order.TonerOrder.OrderDate.Date)
                        {
                            
                        }
                    }
                    var mailer = new Emailer();
                    mailer.subject = MessageList.Open_Toner_Orders_Subject;
                    mailer.body = MessageList.Open_Toner_Orders_Body;
                    mailer.TOList.Add(WebConfigurationManager.AppSettings["SupportEmail"]);
                    mailer.SendEmail();
                    
                
                    contextEf.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        /*
         public static bool ClearOldTonerOrders()
        {
            using (var contextEf = new DataContextEF())
            {
                var openTonerOrders = contextEf.TonerOrderDetails.Where(x => x.OrderStatus == (int)OrderStatus.Opened).ToList();
                if (!openTonerOrders.Any())
                {
                    return false;
                }
                else if(openTonerOrders.Any())
                {
                    for (int i = 0; i < openTonerOrders.Count; i++)
                    {
                        TonerOrderDetailsModel order = openTonerOrders[i];
                        var data = contextEf.TonerOrders.FirstOrDefault(x => x.OrderId.Equals(order.OrderId));
                        if (data != null)
                        {
                            TonerOrdersModel ordersModel = data;
                            openTonerOrders[i].TonerOrder = ordersModel;
                        }
                        if (order.TonerOrder.OrderDate.Date.AddDays(2) > order.TonerOrder.OrderDate.Date)
                        {
                            if (order.TonerOrderReminder < 1)
                            {
                                var mailer = new Emailer();
                                mailer.subject = MessageList.Open_Toner_Orders_Subject;
                                mailer.body = MessageList.Open_Toner_Orders_Body;
                                mailer.TOList.Add(WebConfigurationManager.AppSettings["SupportEmail"]);
                                if (mailer.SendEmail())
                                {
                                    order.TonerOrderReminder++;
                                    order.LastTonerOrderReminder = DateTime.Now;

                                }    
                            }
                            else
                            {
                                order.TonerOrderReminder = 0;
                             
                            }
                            
                        }
                    }
                
                    contextEf.SaveChanges();
                    return true;
                }
            }
            return false;
        }
         */
    }
}
