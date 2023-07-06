# Implementation of Akka.net cluster router based application.

## CartAPI 
This is a starting point of the application which exposes few api's like POST /Cart, POST /{id}/item and GET cart/status. And this dot.net core service internally uses different actor systems to do its job.

## CartPoolRouteehost Actor
 This actor is responsible for processing any request pertaining to cart. We are going to have total 10 cartpoolprocessor actors handled by pool router and it can have 2 actors per/pod. And we can have max 5 pods with each having 2 actors. This is just an example app and this values are set in the code but we can make it configurable.

 ## CartItemPoolRouteeHost
 This is responsible for processing any request pertaining to cart items. We are going to have total 100 cartpoolitemprocessor actor and can have 10 actors per/pod. So we can have max 10 pods each running 10 actors.

 ## State management
 The application uses round robin pool based router for routing the request. When same requets gets routed to different actors, the state across various actor instance is mainatined using akk.net in memory distributed data. We can even persist the data to data store which is not implemented yet.

 ## Deployment
 This has been tested to work with k8s with minikube cluster and as well as with docker compose. So you can deploy it to k8s cluster and make sure you create all the resources in a namespace not on default namespace as a best practice. And you can locally run in visual studio, rider or VS code.






