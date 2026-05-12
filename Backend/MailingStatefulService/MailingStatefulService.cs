using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace MailingStatefulService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MailingStatefulService : StatefulService, IMailingService
    {
        public MailingStatefulService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task PublishEvent(BuyBookRequestDTO buyBookRequestDTO)
        {
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<BuyBookRequestDTO>>("mailingQueue");

            using (var tx = this.StateManager.CreateTransaction()) { 
            
                await queue.EnqueueAsync(tx, buyBookRequestDTO);
                await tx.CommitAsync();

            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {


            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<BuyBookRequestDTO>>("mailingQueue");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await queue.TryDequeueAsync(tx);

                    if (result.HasValue)
                    {
                        

                        var client = new SmtpClient()
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            EnableSsl = true,
                            Credentials = new NetworkCredential("velemirfilip@gmail.com", "cqmz fooh vtkk zzse")
                        };

                        var message = new MailMessage(new MailAddress("velemirfilip@gmail.com", "Filip Velemir"), new MailAddress($"{result.Value.Email}", "Kupac"));




                        message.Subject = "Kupovina uspesno obavljena";
                        message.Body = $"Poštovani,\r\nObavještavamo Vas da je Vaša kupovina uspješno realizovana.\r\nDetalji kupovine:\r\n Naziv knjige: {result.Value.Book.Title}\r\n Količina: {result.Value.Book.Quantity}\r\n" +
                            $" Ukupan iznos: {result.Value.Book.Quantity * result.Value.Book.Price} KM\r\n Datum i vrijeme: {DateTime.Now}\r\nHvala Vam na ukazanom povjerenju.\r\nSrdačan pozdrav,\r\n BookStore tim\r\n";

                        client.Send(message);


                        await tx.CommitAsync();
                    }
                    else
                    {

                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }

                }
            }
        }
    }
}
