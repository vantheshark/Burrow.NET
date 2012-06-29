![Burrow](http://i43.tinypic.com/66bsw7.png)

Burrow.NET, Release 1.0.x (since Mar 12, 2012)
-----------------------------------------------------------------------
* https://github.com/vanthoainguyen/Burrow.NET    
* http://burrow.codeplex.com/


##1. INTRODUCTION

This project is created based on the idea of **EasyNetQ**, since **Mike Hadlow** used _MIT licence_, I hope he doesn't mind when I use his source code in this project.

I was so lucky to have 2 chances to work with RabbitMQ in my 2 recent projects. EasyNetQ is the library I looked into at first place. Honestly, It's a good implementation, the author covered many problems he got with RabbitMQ and I learnt from that as well. However, I created this project for below reasons:

* I need an easier way to define Exchange names and Queue names since I don't like the IConvention in EasyNetQ.
* I want to use Fanout Exchange and I don't need the library so smart to create Exchanges/Queues automatically which EasyNetQ is doing. Indeed, EasyNetQ creates Exchange type Direct everytime a message is published. Not sure if it could affect performance or not but It will throw exception if there is an existing Exchange with same name but different type defined manually. And furthur more, there is no way to override that behavior. Hmmm OCP problem :D
* I want the messages to be consumed parallel. EasyNetQ has a method that looks like it consumes the messages asynchornous but sadly it consume one by one.
* I need more flexibilities to inject behaviors for logging, error handling, object serializing, etc.
* And I want to be busy :D

Alright, to publish a message, you just need something like:

```clj
var tunnel = RabbitTunnel.Factory.Create();
tunnel.Publish(new OrderDetail
{	
    Name = "IPad 3",
    Color = "Black",
    Amount = 1	
});
```

To subscribe:

```clj
var tunnel = RabbitTunnel.Factory.Create();
tunnel.SubscribeAsync<OrderDetail>("SubscriptionKey", msg =>
{
    // Process message here
});
```

Ofcourse you're gonna need a _connection string_ to RabbitMQ server, _exchange_ and _queue_ defined to make it work. Please go to [document page](https://github.com/vanthoainguyen/Burrow.NET/wiki/Get-started-with-Burrow.NET) for more details how to run the test projects.

##2. WHERE TO START?

- Install RabbitMQ
- Create exchange (type **direct*): _Burrow.Exchange_
- Create queue: _Burrow.Queue.BurrowTestApp.Bunny_
- Bind above queue to exchange _Burrow.Exchange_
- Get latest source code
- Run Burrow.Publisher to publish messages
- Run Burrow.Subscriber to subscribe messagages asynchronously from the queue.

##3. DOCUMENT

Documentation can be found at github wiki page: https://github.com/vanthoainguyen/Burrow.NET/wiki/Get-started-with-Burrow.NET    

Some blog posts:

* [Messaging with RabbitMQ and Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/05/messaging-rabbitmq-and-burrownet.html)
* [RabbitMQ Exchanges & Queues naming convention with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/05/rabbitmq-exchange-queue-name-convention.html)
* [Custom Burrow.NET TunnelFactory & RabbitMQ](http://thoai-nguyen.blogspot.com.au/2012/06/custom-burrownet-tunnelfactory-rabbitmq.html)
* [Things you can easily change in Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/06/things-you-can-change-in-burrownet.html)
* [Programmatically  create RabbitMQ Exchange and Queue with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/06/programmatically-rabbitmq-exchange.html)

Nuget library is also added at http://nuget.org/packages/Burrow.NET

##4. LICENCE
http://sam.zoy.org/wtfpl/COPYING 
![Troll](http://i40.tinypic.com/2m4vl2x.jpg) 
