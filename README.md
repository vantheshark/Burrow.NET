![Burrow](http://i43.tinypic.com/66bsw7.png)

Burrow.NET, Release 1.0.x (since Mar 12, 2012)
-----------------------------------------------------------------------
* https://github.com/vanthoainguyen/Burrow.NET    
* http://burrow.codeplex.com/


##1. INTRODUCTION

This project is created based on the idea of **EasyNetQ**, since **Mike Hadlow** used _MIT licence_, I hope he doesn't mind when I use his source code in this project.

I was so lucky to have chances to work with RabbitMQ in my 2 recent companies. EasyNetQ is the library I looked into at first place. Honestly, It's a good implementation, the author covered many problems he got with RabbitMQ and I learnt from that as well. However, I created this project for below reasons:

* I need an easier & flexible way to define Exchange names and Queue names by my conventions
* I need to have different priority queues for messages
* I need my app to fail over to alive RabbitMQ node in a cluster
* I need more flexibilities to inject behaviors for logging, error handling, object serializing, etc.
* And I want to be busy :D
 
Burrow.NET has been supporting all of those and other things, we've been using this library in many apps in our production environment to process million of messages a day.


Alright, to publish a message, you just need something like:

```c#
var tunnel = RabbitTunnel.Factory.Create();
tunnel.Publish(new OrderDetail
{   
	Name = "Google Nexus 7",
	Color = "Black",
	Amount = 1  
});
```

To subscribe:

```c#
var tunnel = RabbitTunnel.Factory.Create();
tunnel.Subscribe(new SubscriptionOption<OrderDetail>
{
	BatchSize = 2,
	MessageHandler = (msg) =>
	{
		// Process message here
	},
	QueuePrefetchSize = 10,
	SubscriptionName = "SubscriptionKey"
});
```

Ofcourse you're gonna need a _connection string_ to RabbitMQ server, _exchange_ and _queue_ defined to make it work. Please go to [document page](https://github.com/vanthoainguyen/Burrow.NET/wiki/Get-started-with-Burrow.NET) for more details how to run the test projects.

Beside Burrow.NET, I have implemented Burrow.Extras and Burrow.RPC which provide some utilities to play with RabbitMQ using C# such as priority queue implementation and RPC.

##2. WHERE TO START?

- Install RabbitMQ
- Create exchange (type **direct*): _Burrow.Exchange_
- Create queue: _Burrow.Queue.BurrowTestApp.Bunny_
- Bind above queue to exchange _Burrow.Exchange_
- Get latest source code
- Run Burrow.Publisher to publish messages
- Run Burrow.Subscriber to subscribe messagages asynchronously from the queue.

##3. DOCUMENT

Documentation can be found at github wiki page: https://github.com/vanthoainguyen/Burrow.NET/wiki/_pages    

Some blog posts:

* [Messaging with RabbitMQ and Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/05/messaging-rabbitmq-and-burrownet.html)
* [RabbitMQ Exchanges & Queues naming convention with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/05/rabbitmq-exchange-queue-name-convention.html)
* [Custom Burrow.NET TunnelFactory & RabbitMQ](http://thoai-nguyen.blogspot.com.au/2012/06/custom-burrownet-tunnelfactory-rabbitmq.html)
* [Things you can easily change in Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/06/things-you-can-change-in-burrownet.html)
* [Programmatically create RabbitMQ Exchange and Queue with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/06/programmatically-rabbitmq-exchange.html)
* [Priority With RabbitMQ implementation in .NET](http://thoai-nguyen.blogspot.com.au/2012/07/priority-queue-rabbitmq-burrownet.html)
* [Implement RPC service using Burrow.NET and RabbitMQ](http://thoai-nguyen.blogspot.com.au/2012/10/rpc-with-burrownet-and-rabbitmq.html)
* [Monitor RabbitMQ queues & count total messages](http://thoai-nguyen.blogspot.com.au/2013/07/monitor-rabbitmq-queues-count-message.html)
* [Using RabbitMQ Header Exchange with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2014/06/using-rabbitmq-header-exchange-with.html)

Nuget library is also added at http://nuget.org/packages/Burrow.NET

##4. LICENCE
http://sam.zoy.org/wtfpl/COPYING 
![Troll](http://i40.tinypic.com/2m4vl2x.jpg) 
