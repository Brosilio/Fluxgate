# WAIT!!!
This is very vulgar.
Read at your own discretion.

Fluxgate asyncronous event based networking libraries for C#/.NET.

# Notes
Fluxgate uses an internal protocol, but don't worry - it's turbo simple.
The first four bytes are considered to be the length of the incoming data.
Fluxgate automatically adds these bytes when you use the Send() method on a client, and automatically removes them when it fires OnFullPacket.

# Getting started
Add `using Fluxgate;` to the top of your file.

## Creating a server
Creating a server is pretty easy. First, just do the following:
```csharp
Server server = new Server(<port>, [backlog]);
```
Then, hook the events provided.
```csharp
server.OnNewClient += OnNewClient;
server.OnError += OnServerError;
```
Or something.
Now, create said methods.
In this case, `OnNewClient(client)` is where we will manage the new clients (duh).
```csharp
public void OnNewClient(Client client)
{
     
}

public void OnServerError(Exception ex) => Console.WriteLine(ex.Message);
```
Once all this is done, you can start the server.
```csharp
server.Listen();
```
## Creating a client
This is also simple. Just do this:
```csharp
Client client = new Client();
```
The client accepts an already connected socket or nothing in the constructor.
The client has many more events to hook than the server.
Since these are really easy to use, I'm going to leave it up to you to figure it out.
This is a list of events on the client:
```
OnFullPacket(Client sender, byte[] data)
OnDisconnect(Client sender, Exception errorIfAny)
OnError(Client sender, Exception fuck)
OnConnect(Client sender)
Log(string info) // This is for internal debugging.
```