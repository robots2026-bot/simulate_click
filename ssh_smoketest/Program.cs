using Renci.SshNet;

const string Host = "192.168.4.50";
const int Port = 22;
const string User = "root";
const string Pass = "lf123!";

try
{
    var connectionInfo = new PasswordConnectionInfo(Host, Port, User, Pass);
    using var ssh = new SshClient(connectionInfo);
    ssh.HostKeyReceived += (_, e) => { e.CanTrust = true; };

    Console.WriteLine($"Connecting to {Host}:{Port}...");
    ssh.Connect();
    Console.WriteLine($"Connected: {ssh.IsConnected}");

    var cmd = ssh.RunCommand("uname -a");
    Console.WriteLine("Command exit status: " + cmd.ExitStatus);
    Console.WriteLine("Stdout:");
    Console.WriteLine(cmd.Result);
    Console.WriteLine("Stderr:");
    Console.WriteLine(cmd.Error);

    ssh.Disconnect();
    Console.WriteLine("Disconnected.");
}
catch (Exception ex)
{
    Console.WriteLine("SSH smoke test failed:");
    Console.WriteLine(ex.ToString());
}
