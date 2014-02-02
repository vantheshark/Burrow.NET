Burrow.NET
========

GitHub https://github.com/vanthoainguyen/Burrow.NET

### What is it?

Burrow.NET is a simple library created based on some EasyNetQ ideas, it's a thin wrapper of RabbitMQ.Client for .NET. Basically, if you just need to put your message or subscribe messages from RabbitMQ server, you found the right place. With Burrow.NET, you can easily customize almost everything start with exchange and queue name, changing the way to serialize your object, inject custom error handling strategies, etc.

### Basic usage

To publish:

	var tunnel = RabbitTunnel.Factory.Create();
	tunnel.Publish(new OrderDetail
	{   
		Name = "Google Nexus 7",
		Color = "Black",
		Amount = 1  
	});

To subscribe:

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


### Need help?

Documentation can be found at github wiki page: https://github.com/vanthoainguyen/Burrow.NET/wiki/Get-started-with-Burrow.NET    



Some blog posts:

* [Messaging with RabbitMQ and Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/05/messaging-rabbitmq-and-burrownet.html)
* [RabbitMQ Exchanges & Queues naming convention with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/05/rabbitmq-exchange-queue-name-convention.html)
* [Custom Burrow.NET TunnelFactory & RabbitMQ](http://thoai-nguyen.blogspot.com.au/2012/06/custom-burrownet-tunnelfactory-rabbitmq.html)
* [Things you can easily change in Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/06/things-you-can-change-in-burrownet.html)
* [Programmatically  create RabbitMQ Exchange and Queue with Burrow.NET](http://thoai-nguyen.blogspot.com.au/2012/06/programmatically-rabbitmq-exchange.html)
* [Priority With RabbitMQ implementation in .NET](http://thoai-nguyen.blogspot.com.au/2012/07/priority-queue-rabbitmq-burrownet.html)
* [RPC With Burrow.NET and RabbitMQ](http://thoai-nguyen.blogspot.com.au/2012/10/rpc-with-burrownet-and-rabbitmq.html)
* [Monitor RabbitMQ queues & count total messages](http://thoai-nguyen.blogspot.com.au/2013/07/monitor-rabbitmq-queues-count-message.html)
* [RabbitMQ queues failover](https://github.com/vanthoainguyen/Burrow.NET/wiki/RabbitMQ-queues-failover)


If you have questions or feedback on Burrow.NET, please post Issue on https://github.com/vanthoainguyen/Burrow.NET/issues

### LICENCE
http://sam.zoy.org/wtfpl/COPYING 

