using Makaretu.Dns;

namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var sd = new ServiceDiscovery();
            //发布一个服务，服务名称是有讲究的，一般都是_开头的，可以找一下相关资料
            var p = new ServiceProfile("test_server", "_zgcwkj._udp", 5010);
            p.AddProperty("connstr", "Server");
            //必须要设置这一项，否则不解析TXT记录
            sd.AnswersContainsAdditionalRecords = true;
            sd.Advertise(p);
            //sd.Announce(p);
            Console.ReadKey();
            sd.Unadvertise();
        }
    }
}
