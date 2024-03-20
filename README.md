[![Static Badge](https://img.shields.io/badge/Shtomper-bf477b?uGet&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FShtomper)](https://www.nuget.org/packages/Shtomper)
[![NuGet](https://img.shields.io/nuget/v/Shtomper.svg)](https://www.nuget.org/packages/Shtomper)
<br>
[![Static Badge](https://img.shields.io/badge/Websocket%20Client-bf477b?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FShtomper.Client.Websocket)](https://www.nuget.org/packages/Shtomper.Client.Websocket)
[![NuGet](https://img.shields.io/nuget/v/Shtomper.Client.Websocket.svg)](https://www.nuget.org/packages/Shtomper.Client.Websocket)
<br>
[![Static Badge](https://img.shields.io/badge/JSON%20Converter-bf477b?link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2FShtomper.Converter.NewtonsoftJson)](https://www.nuget.org/packages/Shtomper.Converter.NewtonsoftJson)
[![NuGet Version](https://img.shields.io/nuget/v/Shtomper.Converter.NewtonsoftJson)](https://www.nuget.org/packages/Shtomper.Converter.NewtonsoftJson)
<br>
[![License](https://img.shields.io/:license-BSD-orange.svg)](https://raw.githubusercontent.com/Cvazer/Shtomper/main/LICENSE)

This is a basic implementation of [STOMP](https://stomp.github.io/index.html) protocol which works over Websocket to transfer lightweight frames.

This repository consists of 3 major parts:
* Abstract layer (**Shtomper**)
* Concrete Websocket Implementation (**Shtomper-Client-Websocket**)
* JSON Converter implementation (**Shtomper-Converter-NewtonsoftJson**)

Shtomper clients are is built upon a layer of abstraction so it would be easy to implement new ones with different underlying transport mechanisms as well as new converters to support different data formats.

### How to use

```StompClientBuilder``` - is a part of Shtomper abstraction layer. A base point to start building a client. It's a builder that contains parameters specific to STOMP protocole.

```WebSocketStompClientFactoryBuilder``` - is a part Shtomper-Client-Websocket client implementation. It's a build that extends ```IStompClientFactoryBuilder``` interface provided by *Shtomper* abstraction and allows to configure implementation specific details (i. e. Websocket details in this case).

``Build(..)`` Method return an instance of ```IStompClientFactory``` specific to given concrete builder (i. e. ```WebSocketStompClientFactory``` for ```WebSocketStompClientFactoryBuilder```) which then can be used to create an instance of ```IStompClient``` which again is specific to builder/factory implementation.

So, in short create shtomper builder => pass *specific* builder implementation to it's ```WithBuilder(..)``` method => build specific factory => create specific client.

#### Example usage

```c#
var waitHandle = new ManualResetEvent(false);

using var client = new StompClientBuilder()
    .SetMessageConverter(new NewtonsoftJsonMessageConverter())
    .SetUsername("guest")
    .SetPasscode("guest")
    .SetHeartbeatCapable(1000)
    .SetHeartbeatDesired(1000)
    .WithBuilder(new WebSocketStompClientFactoryBuilder())
    .SetPort(15674)
    .SetHost("localhost")
    .SetHostOverride("/") //overrides [host] header for CONNECT frame
    .SetPath("ws")
    .Build()
    .Create();

Action<Dictionary<string, string>> callback = Console.WriteLine;

client.Subscribe("/queue/testQueue", callback);

client.Send(
    "/queue/testQueue",
    new Dictionary<string, string> {
        { "firstName", "John" },
        { "lastName", "Doe" }, 
    }
);

waitHandle.WaitOne(TimeSpan.FromSeconds(5));
```

#### **Warning!!!**
Project is in very early stage. If for some reason you wish to use it - be warned.

### Basic Principals

STOMP is a publisher-subscriber type protocol in its core. It defines a message broker (server) that handles message queues\topics (collections of messgaes). Then a subscriber (client) can subscribe to a queue\topic to receive messages that are posted to that specific queue\topic by publishers (another client). Concrete implementation details may vary from broker to broker. The STOMP itself specifies a limited set of operations and frame format and leaves some specifics on broker's shoulders.

Basic workflow might be: *Broker* starts and configures itself => a bunch of *producers* start to put messages into queues => a bunch of *consumers* start to read those messages and do something with them.

In above example a *PRODUCER* is an app that runs STOMP client, connects to the broker and produces some data. it might be a IoT device, a user's web app or any other kind of source of data that you wish to process. Then the *BROKER* is a server that hold data produced by producers and waits for consumers to come and pick it up. *CONSUMERS* are also any kind of general program with a STOMP client that connects to the broker and reads mentioned data and processes it in some way. All data passed to the broker is stored in a separate "pile" so to speak called a *queue* or a *topic*. Whenever a producer passes data to broker it specifies a queue to store it in. Whenever a consumer comes to get the messages it specifies a queue he is *subscribing* to.

STOMP specification support not only basic message handling but also some mechanisms to decline specific message for consumer. Incoming through the subscription message can be acknowledged by the client if this behavior is specified for given subscription. In Shtomper this happens automatically under the hood if corresponding acknowledgement type is specified during subscription, and no acknowledgement happens if an unhandled exception is raised during subscription callback execution. Such message will be considered not consumed as of STOMP specification and might be redelivered to another consumer depending on broker implementation.

Shtomper also supports opposite model, when instead of acknowledging EVERY processed message it REPORTS not processed properly messages to the broker via corresponding STOMP frame (NACK).
<br> 
```c#
using var client = new StompClientBuilder()
    ...
    .SetNackMode(true)
    ...
```

STOMP specification also supports a *Transaction* model that allows to bundle a bunch of messages together and commit them to the broker or abort the transaction and rollback those messages if something goes wrong. Only committed messages will be delivered by the broker to the consumers and aborted ones will be dropped and wil NOT get delivered. Note: Transaction is a SERVER side mechanism. Shtomper fully supports transactions:
```c#
var msg1 = new SomeData();
var tx = client.Transaction();

tx.Send("/queue/someQueue", msg1); // sending msg1. It GOES to the broker NOW

if(..everything good..) 
{
    tx.Commit(); // Broker CLOSES transaction and msg1 STAYS in the queue and gets delivered to consumers
}
else
{
    tx.Abort(); // Broker CLOSES transacyion and msg1 is DISCARDED (not delivered to consumers)
}
```

#### Broker implementation for testing puposes

If you have no desire to write your own broker you can use any out-of-the-box solutions available. Some popular pub-sub model brokers support STOMP out of the box or via plugin

* Shtomper was tested against **RabbitMQ** with STOMP plugin enabled
* **ActiveMQ** supports STOMP without any plugins but it must be enabled via config.
* **Spring Framework**  and by extension **Spring Boot** supports STOMP with it's *Websocket Message Broker* via corresponding starter.
* Other brokers implementing STOMP are listed [*here*](https://stomp.github.io/implementations.html#STOMP_Servers) on official STOMP page
* You can always [*google*](https://www.google.com/search?q=stomp+server) it

### Notes

* All messages are sent asynchronously, from the queue.
* All messages are received asynchronously to the queue where they are being processed by different threads, so your callback will be called on a different thread (obviously)
* STOMP specification mandates a delivery guarantee mode for compliant clients (Receipt). At this time Shtomper DOES NOT support this mode. It IS there in code but I was unable to get it to work fast enough so you can fork it and enable it if you want. But be warned: it REALLY slows everything down with this implementation
* You CAN use string type for generic. It WILL skip the converter call, but you still NEED to provide some kind of no-op placeholder for the builder in place of actual converter
