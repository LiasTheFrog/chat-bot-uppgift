
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

async Task<string> Read(){
    byte[] sentback =  new byte[1024];
var receivedAsync = await pelle.ReceiveAsync(new ArraySegment<byte>(sentback), default);
string recieved = Encoding.UTF8.GetString(sentback,0,receivedAsync.Count);
Console.WriteLine(recieved);
return recieved;
}

async Task HandleKommand(string kommand){

    string kommand1 = "!username";
    string kommand2 = "!joke";
    if(kommand == kommand1){
Console.WriteLine("username: pellekanin");
    }
    else if(kommand == kommand2){
    Console.WriteLine(Parser.getJoke());
    }else{
        Console.WriteLine("kommand doesn't exist");
    }
}



}

class Parser{

public static async Task<string> getJoke(){
JsonSerializerOptions options = new JsonSerializerOptions{WriteIndented = true};

using(HttpClient kalle = new HttpClient()){

 kalle.BaseAddress = new Uri ("https://api.chucknorris.io");

 
try{
    HttpResponseMessage response =  await kalle.GetAsync("jokes/random");
    response.EnsureSuccessStatusCode();

    string responseBody = await response.Content.ReadAsStringAsync();
    string JsonString = JsonSerializer.Serialize(responseBody,options);
    Console.WriteLine(JsonString);
    return JsonString;
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

public static async Task<string> HandleMsg(string user,string msg){
if(msg[0] == '!'){
string joke = await getJoke();

return"";
}else{
Log(user,msg);
return "";
}
}

}


class Program{
    static void Main(){

Parser.getJoke();

    }
}