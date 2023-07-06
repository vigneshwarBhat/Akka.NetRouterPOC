using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Infra;
using Akka.Cluster.Infra.Actor;
using Akka.Cluster.Infra.Events;
using Akka.Cluster.Routing;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Routing;

namespace TradePlacementAPI
{
    public class BridgeActor:ReceiveActor
    {
        private IActorRef _cartRouter;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public BridgeActor(IActorRef actorRef)
        {
            _cartRouter=actorRef;
            Receive<CreateCartRequest>(cartReq =>
            {
                _log.Info($"Received create cart request for cart Id {cartReq.CartId} for the user : {cartReq.UserId}");
                //_shardCartActor.Tell(cartReq);
                _cartRouter.Tell(cartReq);
                Sender.Tell(new CreateCartResponse { CartId = cartReq.CartId, Status = "InProgress" });
            });

            Receive<CreateCartItemRequest>(cartItemReq =>
            {
                _log.Info($"Received add cart item request for item Id {cartItemReq.CartItemId} for the cart : {cartItemReq.CartId}");
                //_shardCartActor.Tell(cartItemReq);
                _cartRouter.Tell(cartItemReq);
                Sender.Tell(new CreateCartItemResponse { CartItemId = cartItemReq.CartItemId, Status="InProgress" });
            });


            Receive<GetCartStatus>(async GetCartStatus =>
            {
                var sender = Context.Sender;
                //var result = await _shardCartActor.Ask<CartJournal>(GetCartStatus);
                var result=await _cartRouter.Ask<CartJournal>(GetCartStatus);
                _log.Info($"Received Get cart status response for cart Id: {result.CartId} with status : {result.CartStatus}");
                sender.Tell(result);
                
            });
        }

        // protected override void PreStart()
        // {
        //     Cluster.Get(Context.System).RegisterOnMemberUp(() =>
        //     {
        //         var cartRouter = Context.System.ActorOf(Props.Create(() => new CartItemProcessor())
        //          .WithRouter(new ClusterRouterPool(new RoundRobinPool(100), new ClusterRouterPoolSettings(100, 10, false, "CartItemRouteeHost"))), "cartItemPoolRouter");

        //         if (Context.Child("cartPoolRouter").Equals(ActorRefs.Nobody))
        //         {
        //             _cartRouter = Context.System.ActorOf(Props.Create(() => new CartProcessor(cartRouter)).WithRouter(new ClusterRouterPool(new RoundRobinPool(10), new ClusterRouterPoolSettings(10, 2, false, "CartRouteeHost"))), "cartPoolRouter");
        //         }
        //         else
        //         {
        //             _cartRouter = Context.Child("cartPoolRouter");
        //         }                          
        //     });
           
        //     base.PreStart();
        // }

        // protected override void PostStop()
        // {
        //     base.PostStop();
        // }

    }
}
