using Makaretu.Dns;
using System.Net.Sockets;

namespace DemoConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using var mdns = new MulticastService();
            using var sd = new ServiceDiscovery(mdns);

            //服务实例发现
            sd.ServiceInstanceDiscovered += (s, e) =>
            {
                if (e.Message.Answers.All(w => !w.Name.ToString().Contains("test_server"))) return;
                Console.WriteLine($"service instance '{e.ServiceInstanceName}'");
                //询问服务实例详细信息
                mdns.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
            };
            //收到答复
            mdns.AnswerReceived += (s, e) =>
            {
                if (e.Message.Answers.All(w => !w.Name.ToString().Contains("test_server"))) return;
                //这是对服务实例详细信息的回答吗？
                var servers = e.Message.Answers.OfType<SRVRecord>();
                foreach (var server in servers)
                {
                    Console.WriteLine($"host '{server.Target}' for '{server.Name}'");

                    //询问主机 IP 地址
                    mdns.SendQuery(server.Target, type: DnsType.A);
                    //mdns.SendQuery(server.Target, type: DnsType.AAAA);
                }

                //这是对主机地址的答案吗？
                var addresses = e.Message.Answers.OfType<AddressRecord>();
                foreach (var address in addresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                        Console.WriteLine($"host '{address.Name}' at {address.Address}");
                }
                //从 DNS TXT 记录获取连接字符串。
                var txts = e.Message.Answers.OfType<TXTRecord>();
                foreach (var txt in txts)
                {
                    Console.WriteLine($"{txt.Strings.Single(w => w.Contains("connstr")).Split('=')[1]}");
                }
            };

            try
            {
                mdns.Start();
                sd.QueryServiceInstances("_zgcwkj._udp");
                Console.ReadKey();
            }
            finally
            {
                sd.Dispose();
                mdns.Stop();
            }
            Console.ReadKey();
        }
    }
}
