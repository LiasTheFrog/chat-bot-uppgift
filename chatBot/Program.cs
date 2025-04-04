
using System.Data.Common;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

class Bot{
Uri wss = new Uri("wss://irc-ws.chat.twitch.tv:443");
ClientWebSocket pelle = new ClientWebSocket();

async Task Connect(){
await pelle.ConnectAsync(wss,default);
Console.WriteLine("connected");
}

async Task<bool> ConfirmID(){
string[] ID = PersonalInfo();
await verify(ID[0],ID[1]);
string respons = await Read();
if(respons != ":tmi.twitch.tv NOTICE * :Login authentication failed"){
return true;
}else{
    return false;
}
}

string[] PersonalInfo(){
    StreamReader info = new StreamReader("personal-info.txt");
    string infoLine1 = info.ReadLine();
    string infoLine2 = info.ReadLine();
    string[] infoFromFile = [infoLine1, infoLine2];
return infoFromFile;

}

async Task verify(string IdInfo1, string IdInfo2){
    byte[] usernameBytes = Encoding.UTF8.GetBytes(IdInfo1);
    byte[] oauthBytes = Encoding.UTF8.GetBytes(IdInfo2);
    ArraySegment<byte> username = new(usernameBytes);
    ArraySegment<byte> oauth = new(oauthBytes);

await pelle.SendAsync(username,WebSocketMessageType.Text, true, CancellationToken.None);
await pelle.SendAsync(oauth,WebSocketMessageType.Text, true, CancellationToken.None);
Console.WriteLine("verification sent");
}

async Task Write(string msg){
    byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
    ArraySegment<byte> message = new(msgBytes);
    await pelle.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
    Console.WriteLine("message sent");
    await Read();
    
}

public async Task<string> Read(){
    byte[] sentback =  new byte[1024];
var receivedAsync = await pelle.ReceiveAsync(new ArraySegment<byte>(sentback), default);
string recieved = Encoding.UTF8.GetString(sentback,0,receivedAsync.Count);
Console.WriteLine(recieved);
return recieved;
}

public async Task HandleKommand(string kommand, string[] args){

    string kommand1 = "!rev";
    string kommand2 = "!joke";
    if(kommand == kommand1){



    }
    else if(kommand == kommand2){
    Console.WriteLine(await Parser.getJoke());
    }else{
        Console.WriteLine("kommand doesn't exist");
    }
}



}

class Parser{

public static string removeFromPrefix(char prefix,string input){
    string[] removeChar = input.Split(prefix);
     
return removeChar[1];
}

public static string getCommand(char prefix,string input){
    string command = removeFromPrefix(prefix,input);
    string[] removeChar = input.Split(' ');
    string removed = removeFromPrefix('!', removeChar[0]);
        Console.WriteLine($" {removed} ");

    return removed;
}

    public static string[] getArgs(string input){
    string[] removeChar = input.Split(' ');
    return removeChar[1..];
}

public static async Task<string> getJoke(){
JsonSerializerOptions options = new JsonSerializerOptions{WriteIndented = true};

using(HttpClient kalle = new HttpClient()){

 kalle.BaseAddress = new Uri ("https://api.chucknorris.io");

 
try{
    HttpResponseMessage response =  await kalle.GetAsync("jokes/random");
    Console.WriteLine("#");
    response.EnsureSuccessStatusCode();

    string responseBody = await response.Content.ReadAsStringAsync();
    string JsonString = JsonSerializer.Serialize(responseBody,options);

    /* Console.WriteLine(JsonString); */
    string[] splitjoke = JsonString.Split("value");
string substringjoke = splitjoke[1].Substring(13,splitjoke[1].Length - 21);
byte[] byteMSG = Encoding.UTF8.GetBytes(substringjoke);
string stringMSG = Encoding.UTF8.GetString(byteMSG);
    return stringMSG;
}
catch(HttpRequestException e){
    Console.WriteLine(e.Message);
    return e.Message;
}
}

}


public static string Log(string user, string msg){

using(StreamWriter LogText = new StreamWriter("log.txt")){
    LogText.WriteLine(msg);
    return msg;
}

}

public static bool HandleMsg(string msg){
return msg[0] == '!';

}

}


class Program{
    static async Task Main(){
bool running = true;
Bot twitch = new Bot();

while(running){

/* string msg = await twitch.Read(); */
string msg = "!joke";
if(Parser.HandleMsg(msg)){
    Console.WriteLine(Parser.removeFromPrefix('!', msg));
}



}



    }
}