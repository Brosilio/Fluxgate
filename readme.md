Fluxgate asyncronous event based networking libraries for C#/.NET.

# Notes
Fluxgate uses an internal protocol, but don't worry - it's turbo simple.
The first four bytes are considered to be the length of the incoming data.
I don't remember if it discards these before firing the OnData event, so watch out for a stray four bytes.
Sometimes random shit gets appended to packets, so just remove the last byte if it's broke.

# Getting started
First, y'all must `using Fluxgate;` 'n stuff.

## Creating a server
Creating a server is pretty fr*cking easy. First, just fucking do the following:
```csharp
Server server = new Server(<port>, [backlog]);
```
Then, hook the events for good shit.
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
Once all this shit is done, you can start the server.
```csharp
server.Listen();
```
## Creating a client
This is also simple af. Just do this:
```csharp
Client client = new Client();
```
The client accepts an already connected socket or nothing in the constructor.
The client has many more events to hook than the server, so fuckle your seatbelts and don't piss off.
Since these are really easy to use, I'm going to leave it up to you to figure it out.
This is a list of events on the client:
```
OnFullPacket(Client sender, byte[] data)
OnDisconnect(Client sender, Exception errorIfAny)
OnError(Client sender, Exception fuck)
OnConnect(Client sender)
Log(string info) // this isn't used but I forgot to remove it before I pushed to repo
```