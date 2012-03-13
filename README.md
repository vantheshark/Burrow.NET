![Burrow](http://i43.tinypic.com/66bsw7.png)

Burrow.NET, Release 1.0.x (since March 12, 2012)
-----------------------------------------------------------------------
https://github.com/vanthoainguyen/Burrow.NET
http://burrow.codeplex.com/


##1. INTRODUCTION

This project is created based on the idea of **EasyNetQ**, since **Mike Hadlow** used _MIT licence_, I hope he doesn't mind when I use his source code in this project.

I was so lucky to have 2 chances to work with RabbitMQ in my 2 recent projects. EasyNetQ is the library I looked into at first place. Honestly, It's a good implementation, the author covered many problems he got with RabbitMQ and I learnt from that as well. However, I created this project for below reasons:

	* I need an easier way to define Exchange names and Queue names since I don't like the IConvention in EasyNetQ.
	* I need Fanout Exchange and I don't need the library to create Exchange/Server automatically which EasyNetQ is doing. Indeed, EasyNetQ creates Exchange type Direct everytime a message is published. Not sure if it could affect performance or not but It will throw exception because there is an existing Exchange with same name but different type defined manually. And furthur more, there is no way to override that behavior. Hmmm OCP problem :D
	* I want the messages to be consumed parallel. EasyNetQ has a method that looks like it consumes the messages asynchornous but sadly it consume one by one.
	* I need more flexibilities to inject behaviors for logging, error handling, object serializing, etc
	* And I want to be busy.

Alright, to publish a message, you just need something like:
var tunnel = TunnelFactory.Create();

```clj
tunnel.Publish(new OrderDetail
{	
    Name = "IPad 3",
    Color = "Black",
    Amount = 1	
});
```

To subscribe:

```clj
var tunnel = TunnelFactory.Create();
tunnel.SubscribeAsync<OrderDetail>("SubscriptionKey", msg =>
{
    // Process message here
});
```

Ofcourse you're gonna need a connection string to RabbitMQ server, exchange and queue defined to make it work. Please go to document page for more details how to run the test project.

##2. WHERE TO START?

- Install RabbitMQ
- Create exchange (type direct): Burrow.Exchange
- Create queue: Burrow.Queue.BurrowTestApp.Bunny
- Bind above queue to exchange Burrow.Exchange
- Download source code
- Run Burrow.Publisher to publish messages
- Run Burrow.Subscriber to subscribe messagages asynchronously from queue.

Documentation can be found at github wiki page: https://github.com/vanthoainguyen/Burrow.NET/wiki/Get-started-with-Burrow.NET or at Codeplex project page: http://burrow.codeplex.com/documentation

Nuget library is also added at http://nuget.org/packages/Burrow.NET

##3. LICENCE
http://sam.zoy.org/wtfpl/COPYING 
![Troll](http://i40.tinypic.com/2m4vl2x.jpg) 
