using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Fluxgate
{
    public class Client : IDisposable
    {
        public Socket clientSocket;

        /// <summary>
        /// Fired when a full packet is received.
        /// TODO: Support multiple subscribers
        /// </summary>
        public Action<byte[], Client> OnFullPacket;

        /// <summary>
        /// Fired when this client disconnects.
        /// Includes the exception, if any.
        /// TODO: Support multiple subscribers
        /// </summary>
        public Action<Client, Exception> OnDisconnect;

        /// <summary>
        /// Fired upon non-disconnect related errors.
        /// TODO: Support multiple subscribers
        /// </summary>
        public Action<Client, Exception> OnError;

        /// <summary>
        /// Fired when this client connects.
        /// TODO: Support multiple subscribers
        /// </summary>
        public Action<Client> OnConnect;

        /// <summary>
        /// Unused.
        /// TODO: Support multiple subscribers
        /// </summary>
        public Action<string> log;

        public bool IsConnected
        {
            get
            {
                return clientSocket != null && clientSocket.Connected;
            }
        }

        public UserProperties UserProperties { get; private set; }

        public Client(Socket clientSocket)
        {
            UserProperties = new UserProperties();
            this.clientSocket = clientSocket;
            Console.WriteLine("FUCK");
            log?.Invoke("shit fuck");
            StartReceiving();
        }

        public Client()
        {
            UserProperties = new UserProperties();
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            clientSocket.SendBufferSize = 32768;
            clientSocket.ReceiveBufferSize = 32768;
        }

        public void Connect(string host, int port)
        {
            try
            {
                clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                clientSocket.BeginConnect(host, port, new AsyncCallback(ConnectCallback), clientSocket);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);
                StartReceiving();
                OnConnect?.Invoke(this);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        public void Disconnect()
        {
            clientSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), this);
            Dispose();
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            InvokeOnDisconnect(null);
            try
            {
                clientSocket.EndDisconnect(ar);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        private void InvokeOnDisconnect(Exception exception)
        {
            OnDisconnect?.Invoke(this, exception);
            Dispose();
        }

        private void StartReceiving()
        {
            try
            {
                StateObject state = new StateObject();
                clientSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                int bytesRead = clientSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.receivedBytes.AddRange(state.buffer.Take(bytesRead));
                    log?.Invoke("some bytes got");
                    if (state.receivedBytes.Count >= 4)
                    {
                        if (state.dataSize == -1)
                        {
                            state.dataSize = BitConverter.ToInt32(state.receivedBytes.ToArray(), 0);
                            log?.Invoke("Should probably be getting " + state.dataSize + " bytes");
                        }
                        if (state.receivedBytes.Count >= state.dataSize)
                        {
                            OnFullPacket?.Invoke(state.receivedBytes.ToArray(), this);
                            state.Reset();
                            clientSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                        }
                        else
                        {
                            clientSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                        }
                    }
                    else
                    {
                        clientSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                    }
                }else if(bytesRead == 0)
                {
                    InvokeOnDisconnect(null);
                    state.Reset();
                }
            }
            catch(Exception ex)
            {
                InvokeOnDisconnect(ex);
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                log?.Invoke("Sending a calculated " + (data.Length + 4) + " bytes");
                List<byte> newBytes = new List<byte>(data.Length + 4);
                newBytes.AddRange(BitConverter.GetBytes((data.Length + 4)));
                newBytes.AddRange(data);
                clientSocket.BeginSend(newBytes.ToArray(), 0, newBytes.Count, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);
                log?.Invoke("Sending an actual" + newBytes.Count + " bytes");
            }
            catch(Exception ex)
            {
                InvokeOnDisconnect(ex);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                InvokeOnDisconnect(ex);
            }
        }

        private void OnFullPacketCallback(IAsyncResult ar)
        {
            try
            {
                OnFullPacket?.EndInvoke(ar);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    clientSocket.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Client() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class StateObject
    {
        public const int BUFFER_SIZE = 32768;
        public int dataSize = -1;
        public byte[] buffer = new byte[BUFFER_SIZE];
        public List<byte> receivedBytes = new List<byte>();
        public void Reset()
        {
            receivedBytes.Clear();
            dataSize = -1;
            buffer = new byte[BUFFER_SIZE];
        }
    }
}