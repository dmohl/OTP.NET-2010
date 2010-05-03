/*``The contents of this file are subject to the Erlang Public License,
* Version 1.1, (the "License"); you may not use this file except in
* compliance with the License. You should have received a copy of the
* Erlang Public License along with this software. If not, it can be
* retrieved via the world wide web at http://www.erlang.org/.
* 
* Software distributed under the License is distributed on an "AS IS"
* basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See
* the License for the specific language governing rights and limitations
* under the License.
* 
* The Initial Developer of the Original Code is Ericsson Utvecklings AB.
* Portions created by Ericsson are Copyright 1999, Ericsson Utvecklings
* AB. All Rights Reserved.''
* 
 * Converted from Java to C# by Vlad Dumitrescu (vlad_Dumitrescu@hotmail.com)
*/
namespace Otp
{
	using System;

	/*
	* Represents an OTP node. It is used to connect to remote nodes or
	* accept incoming connections from remote nodes.
	* 
	* <p> When the C# node will be connecting to a remote Erlang, C#
	* or C node, it must first identify itself as a node by creating an
	* instance of this class, after which it may connect to the remote
	* node.
	*
	* <p> When you create an instance of this class, it will bind a
	* socket to a port so that incoming connections can be accepted.
	* However the port number will not be made available to other nodes
	* wishing to connect until you explicitely register with the port
	* mapper daemon by calling {@link #publishPort()}. </p>
	*
	* <pre>
	* OtpSelf self = new OtpSelf("client","authcookie");  // identify self
	* OtpPeer other = new OtpPeer("server"); // identify peer
	* 
	* OtpConnection conn = self.connect(other); // connect to peer
	* </pre>
	*
	**/
	public class OtpSelf:OtpLocalNode
	{
		private System.Net.Sockets.TcpListener sock;
		private Erlang.Pid _pid;
		
		/*
		* <p> Create a self node using the default cookie. The default
		* cookie is found by reading the first line of the .erlang.cookie
		* file in the user's home directory. The home directory is obtained
		* from the System property "user.home". </p>
		*
		* <p> If the file does not exist, an empty string is used. This
		* method makes no attempt to create the file. </p>
		*
		* @param node the name of this node.
		*
		**/
		public OtpSelf(System.String node):this(node, defaultCookie, 0)
		{
		}
		
		/*
		* Create a self node.
		*
		* @param node the name of this node.
		*
		* @param cookie the authorization cookie that will be used by this
		* node when it communicates with other nodes.
		**/
		public OtpSelf(System.String node, System.String cookie):this(node, cookie, 0)
		{
		}
		
		public OtpSelf(System.String node, System.String cookie, int port):base(node, cookie)
		{
			
			System.Net.Sockets.TcpListener temp_tcplistener;
			//UPGRADE_NOTE: This code will be optimized in the future;
			temp_tcplistener = new System.Net.Sockets.TcpListener(System.Net.Dns.GetHostEntry("localhost").AddressList[0], port);
			temp_tcplistener.Start();
			sock = temp_tcplistener;
			
			if (port != 0)
				this._port = port;
			else
			{
				this._port = ((System.Net.IPEndPoint)sock.LocalEndpoint).Port;
			}
			
			this._pid = createPid();
		}
		
		/*
		* Get the Erlang PID that will be used as the sender id in all
		* "anonymous" messages sent by this node. Anonymous messages are
		* those sent via send methods in {@link OtpConnection
		* OtpConnection} that do not specify a sender. 
		*
		* @return the Erlang PID that will be used as the sender id in
		* all anonymous messages sent by this node.
		**/
		public virtual Erlang.Pid pid()
		{
			return _pid;
		}
		
		
		/*
		* Make public the information needed by remote nodes that may wish
		* to connect to this one. This method establishes a connection to
		* the Erlang port mapper (Epmd) and registers the server node's
		* name and port so that remote nodes are able to connect.
		*
		* <p> This method will fail if an Epmd process is not running on
		* the localhost. See the Erlang documentation for information about
		* starting Epmd.
		*
		* <p> Note that once this method has been called, the node is
		* expected to be available to accept incoming connections. For that
		* reason you should make sure that you call {@link #accept()}
		* shortly after calling {@link #publishPort()}. When you no longer
		* intend to accept connections you should call {@link
		* #unPublishPort()}.
		*
		* @return true if the operation was successful, false if the node
		* was already registered.
		*
		* @exception C#.io.IOException if the port mapper could not be contacted.
		**/
		public virtual bool publishPort()
		{
			if (getEpmd() != null)
				return false;
			// already published
			
			OtpEpmd.publishPort(this);
			return (getEpmd() != null);
		}
		
		
		/*
		* Unregister the server node's name and port number from the Erlang
		* port mapper, thus preventing any new connections from remote
		* nodes.
		**/
		public virtual void  unPublishPort()
		{
			// unregister with epmd
			OtpEpmd.unPublishPort(this);
			
			// close the local descriptor (if we have one)
			try
			{
				if (base.epmd != null)
					base.epmd.Close();
			}
			catch (System.IO.IOException)
			{
				/*ignore close errors */
			}
			base.epmd = null;
		}
		
		/*
		* Accept an incoming connection from a remote node. A call to this
		* method will block until an incoming connection is at least
		* attempted.
		*
		* @return a connection to a remote node.
		*
		* @exception C#.io.IOException if a remote node attempted to
		* connect but no common protocol was found.
		*
		* @exception OtpAuthException if a remote node attempted to
		* connect, but was not authorized to connect.
		**/
		public virtual OtpConnection accept()
		{
			System.Net.Sockets.TcpClient newsock = null;
			
			while (true)
			{
				try
				{
					newsock = sock.AcceptTcpClient();
					return new OtpConnection(this, newsock);
				}
				catch (System.IO.IOException e)
				{
					try
					{
						if (newsock != null)
							newsock.Close();
					}
					catch (System.IO.IOException)
					{
						/*ignore close errors */
					}
					throw e;
				}
			}
		}
		
		/*
		* Open a connection to a remote node.
		*
		* @param other the remote node to which you wish to connect.
		*
		* @return a connection to the remote node.
		*
		* @exception C#.net.UnknownHostException if the remote host could
		* not be found.
		*
		* @exception C#.io.IOException if it was not possible to connect
		* to the remote node.
		*
		* @exception OtpAuthException if the connection was refused by the
		* remote node.
		**/
		public virtual OtpConnection connect(OtpPeer other)
		{
			return new OtpConnection(this, other);
		}
	}
}