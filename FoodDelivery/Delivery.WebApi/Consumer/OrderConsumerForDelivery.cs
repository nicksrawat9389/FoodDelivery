using Delivery.WebApi.IRepository;
using Delivery.WebApi.Models;
using MassTransit;
using Newtonsoft.Json;
using SharedService;

namespace Delivery.WebApi.Consumer
{
    public class OrderConsumerForDelivery : IConsumer<OrderPlaced>
    {
        private readonly IAssignDeliveryRepository _assignDeliveryRepository;


        public OrderConsumerForDelivery(IAssignDeliveryRepository assignDeliveryRepository)
        {
            _assignDeliveryRepository = assignDeliveryRepository;
        }
        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            DeliveryPartner deliveryPartner = await _assignDeliveryRepository.AvailableDeliveryPartner();
            if (deliveryPartner != null)
            {
                AssignedDelivery assignedDelivery = new AssignedDelivery(context.Message.OrderId, deliveryPartner.DeliveryPartnerName, deliveryPartner.PhoneNumber, deliveryPartner.Id);
                await _assignDeliveryRepository.MakeDeliveryPartnerFreeAndBusy(deliveryPartner.Id, false);
                await Task.Delay(TimeSpan.FromSeconds(25));
                await _assignDeliveryRepository.OrderDelivered(assignedDelivery);
                await _assignDeliveryRepository.MakeDeliveryPartnerFreeAndBusy(deliveryPartner.Id,true);

            }
            else
            {

                await context.Redeliver(TimeSpan.FromSeconds(10));
            }
        }
    }
}
