#!/usr/bin/python
import socket
import threading
import json
import sys


if len(sys.argv) < 5:
    sys.exit("usage: ./httpProxy.py <ProxyHost> <ProxyPort> <ResolverHost> <ResolverPort>")
# get Parameters
ProxyHost = sys.argv[1]
ProxyPort = int(sys.argv[2])
ResolverHost = sys.argv[3]
ResolverPort = int(sys.argv[4])


class HttpProxy:
    def __init__(self):
        self.serverSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.serverSocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.serverSocket.bind((ProxyHost, ProxyPort))
        self.serverSocket.listen(10)

        while True:
            (clientSocket, client_address) = self.serverSocket.accept()
            t = threading.Thread(name=client_address,
                                 target=self.proxy_thread, args=(clientSocket, client_address))
            t.setDaemon(True)
            t.start()

    def proxy_thread(self, client_conn, addr):
        try:
            request = client_conn.recv(1024).decode("utf-8")
            print("----------------- Client:", addr)
            print(request)

            url = request.split('\n')[0].split(' ')[1]
            http_pos = url.find("://")
            if http_pos == -1:
                temp = url
            else:
                temp = url[(http_pos + 3):]

            port_pos = temp.find(":")
            webserver_pos = temp.find("/") if temp.find("/") != -1 else len(temp)
            if port_pos == -1 or webserver_pos < port_pos:
                port = 80
                webserver = temp[:webserver_pos]
            else:
                port = int((temp[(port_pos + 1):])[:webserver_pos - port_pos - 1])
                webserver = temp[:port_pos]

            ##########
            # do DNS query
            query = {"dns.flags.response": 0, "dns.qry.name": webserver, "dns.qry.type": 1,  "dns.flags.recdesired": 1}
            try:
                st = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                st.connect((ResolverHost, ResolverPort))
                st.sendall(bytes(json.dumps(query), encoding="utf-8"))
            except ConnectionError:
                print('\033[91m' + "Resolver not available" + '\033[0m')
                sys.exit()

            response = json.loads(st.recv(1024))
            host_ip = response["dns.a"] if response["dns.a"] else False
            #########

            s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            # if local name resolution fails do a normal DNS Request and get the Content from the responseIp
            s.connect((host_ip or webserver, port))
            s.sendall(bytes(request, encoding="utf-8"))

            while 1:
                data = s.recv(1024)
                if len(data) > 0:
                    client_conn.send(data)
                else:
                    break
        except UnicodeDecodeError:
            print('\033[91m' + "Unidentified Format - using secure protocol ?" + '\033[0m')
