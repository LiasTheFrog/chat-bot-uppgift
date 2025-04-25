
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
Uri wss = new Uri("wss://echo.websocket.in");
ClientWebSocket pelle = new ClientWebSocket();

public async Task Connect(){
await pelle.ConnectAsync(wss,default);
Console.WriteLine("connected");
}

public async Task<bool> ConfirmID(){
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
Read();
await pelle.SendAsync(oauth,WebSocketMessageType.Text, true, CancellationToken.None);
Read();
Console.WriteLine("verification sent");
}

public async Task<string> Write(string msg){
    Parser.Log("pelle", msg);
    byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
    ArraySegment<byte> message = new(msgBytes);
    await pelle.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
    Console.WriteLine("message sent");
    return await Read();
    
}

public async Task<string> Read(){
    byte[] sentback =  new byte[1024];
var receivedAsync = await pelle.ReceiveAsync(new ArraySegment<byte>(sentback), default);
string recieved = Encoding.UTF8.GetString(sentback,0,receivedAsync.Count);
Console.WriteLine(recieved);
Parser.Log("Echo",recieved);
return recieved;
}

public async Task<string> HandleKommand(string kommand, string[] args){

    string kommand1 = "rev";
    string kommand2 = "joke";
    Console.WriteLine($"kommando är: {kommand}");
    if(kommand == kommand1){

char[] cArray = args[0].ToCharArray();
string s = "";

for(int i = cArray.Count() - 1; i >= 0; i--){
    s += cArray[i];
    
}
return s;

    }
    else if(kommand == kommand2){
    Console.WriteLine();
    return await Parser.getJoke();
    }
    else{
        
        return "kommand doesn't exist";
    }
}



}

class Parser{

public static string removeFromPrefix(char prefix,string input){
    string[] removeChar = input.Split(prefix);
     Console.WriteLine(removeChar[1]);
return removeChar[1];
}

public static string getCommand(char prefix,string input){
    string command = removeFromPrefix(prefix,input);
    string[] removeChar = command.Split(' ');
    return removeChar[0];
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
    LogText.WriteLine($"[{DateTime.Now}], {user}: {msg}");
    
}
return msg;
}

public static bool HandleMsg(string msg){
return msg[0] == '!';

}

}


class Program{
    static async Task Main(){
bool running = true;
Bot twitch = new Bot();
await twitch.Connect();
await twitch.ConfirmID();

while(running){

Console.WriteLine("skriv meddelande: ");
string input = Console.ReadLine();



if(Parser.HandleMsg(input)){
    Console.WriteLine(await twitch.HandleKommand(Parser.getCommand('!', input), Parser.getArgs(input)));
    running = false;
}else{
   
string msg = await twitch.Write(input);
}



}



    }
}