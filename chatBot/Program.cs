
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
/// <summary>
/// Kopplar användaren till wss servern.
/// </summary>
/// <returns></returns>
public async Task Connect(){
await pelle.ConnectAsync(wss,default);

}
/// <summary>
/// Tar användarinfo från en fil och skickar det till servern för konfirmation av identiteten av användaren.
/// </summary>
/// <returns>returnerar true eller false.</returns>
public async Task<bool> ConfirmID(){
string[] id = PersonalInfo();
await verify(id[0],id[1]);
string respons = await Read();
if(respons != ":tmi.twitch.tv NOTICE * :Login authentication failed"){
return true;
}else{
    return false;
}
}
/// <summary>
/// Hämtar användarinfon från fil.
/// </summary>
/// <returns>returnerar infon från filen.</returns>
string[] PersonalInfo(){
    StreamReader info = new StreamReader("personal-info.txt");
    string infoLine1 = info.ReadLine();
    string infoLine2 = info.ReadLine();
    string[] infoFromFile = [infoLine1, infoLine2];
return infoFromFile;

}
/// <summary>
/// Kollar om användarinfon stämmer.
/// </summary>
/// <param name="idInfo1">användarnamnet.</param>
/// <param name="idInfo2">användarens lösenord.</param>
/// <returns></returns>
async Task verify(string idInfo1, string idInfo2){
    byte[] usernameBytes = Encoding.UTF8.GetBytes(idInfo1);
    byte[] oauthBytes = Encoding.UTF8.GetBytes(idInfo2);
    ArraySegment<byte> username = new(usernameBytes);
    ArraySegment<byte> oauth = new(oauthBytes);

await pelle.SendAsync(username,WebSocketMessageType.Text, true, CancellationToken.None);
Read();
await pelle.SendAsync(oauth,WebSocketMessageType.Text, true, CancellationToken.None);
Read();

}
/// <summary>
/// skriver meddelanden till en logfil.
/// </summary>
/// <param name="msg">meddelandet som användaren har skrivit.</param>
/// <returns></returns>
public async Task<string> Write(string msg){
    Parser.Log("pelle", msg);
    byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
    ArraySegment<byte> message = new(msgBytes);
    await pelle.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
    
    return await Read();
    
}
/// <summary>
/// tar emot meddelanden från servern och loggar dem till logfilen.
/// </summary>
/// <returns>returnerar meddelanderna från servern.</returns>
public async Task<string> Read(){
    byte[] sentback =  new byte[1024];
var receivedAsync = await pelle.ReceiveAsync(new ArraySegment<byte>(sentback), default);
string recieved = Encoding.UTF8.GetString(sentback,0,receivedAsync.Count);

Parser.Log("Echo",recieved);
return recieved;
}
/// <summary>
/// kollar vilket kommando som användaren har skrivit om något alls.
/// </summary>
/// <param name="command">kommando som användaren har skrivit.</param>
/// <param name="args">allt som inte är kommando.</param>
/// <returns>returnerar ett skämt, omvänd sträng eller en sträng som säger att ett visst kommando inte finns.</returns>
public async Task<string> HandleKommand(string command, string[] args){

    string command1 = "rev";
    string command2 = "joke";
    
    if(command == command1){

char[] cArray = args[0].ToCharArray();
string s = "";

for(int i = cArray.Count() - 1; i >= 0; i--){
    s += cArray[i];
    
}
return s;

    }
    else if(command == command2){
    
    return await Parser.getJoke();
    }
    else{
        
        return "kommand doesn't exist";
    }
}



}

class Parser{
/// <summary>
/// Tar bort utropstecket från kommandot.
/// </summary>
/// <param name="prefix"></param>
/// <param name="input"></param>
/// <returns>returnerar kommandot utan utropstecknet</returns>
public static string removeFromPrefix(char prefix,string input){
    string[] removeChar = input.Split(prefix);
   
return removeChar[1];
}
/// <summary>
/// delar upp meddelandet för att extrahera kommandot ur det.
/// </summary>
/// <param name="prefix"></param>
/// <param name="input"></param>
/// <returns>returnerar kommandot med utropstecknet.</returns>
public static string getCommand(char prefix,string input){
    string command = removeFromPrefix(prefix,input);
    string[] removeChar = command.Split(' ');
    return removeChar[0];
}
/// <summary>
/// extraherar allt som inte är ett kommando hur meddelandet.
/// </summary>
/// <param name="input"></param>
/// <returns>returnerar allt som inte är ett kommando</returns>
    public static string[] getArgs(string input){
    string[] removeChar = input.Split(' ');
    return removeChar[1..];
}
/// <summary>
/// hämtar ett chucknorris skämt från internet.
/// </summary>
/// <returns>returnerar det hämtade skämtet.</returns>
public static async Task<string> getJoke(){
JsonSerializerOptions options = new JsonSerializerOptions{WriteIndented = true};

using(HttpClient kalle = new HttpClient()){

 kalle.BaseAddress = new Uri ("https://api.chucknorris.io");

 
try{
    HttpResponseMessage response =  await kalle.GetAsync("jokes/random");
    
    response.EnsureSuccessStatusCode();

    string responseBody = await response.Content.ReadAsStringAsync();
    string jsonString = JsonSerializer.Serialize(responseBody,options);

    string[] splitjoke = jsonString.Split("value");
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

/// <summary>
/// skriver meddelande till logfilen.
/// </summary>
/// <param name="user">användarnamnet</param>
/// <param name="msg">meddelandet som användaren har skrivit.</param>
/// <returns>returnerar meddelandet.</returns>
public static string Log(string user, string msg){

using(StreamWriter logText = new StreamWriter("log.txt", true)){
    logText.WriteLine($"[{DateTime.Now}], {user}: {msg}");
    
}
return msg;
}
/// <summary>
/// kollar om ett meddelande innehåller ett kommando.
/// </summary>
/// <param name="msg">meddelandet från användaren.</param>
/// <returns>returnerar true eller false om meddelandet innehåller ett utropstecken.</returns>
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
Console.WriteLine("connected");
while(running){

Console.WriteLine("skriv meddelande: ");
string input = Console.ReadLine();

if(input != ""){


if(input == "!quit"){
    Parser.Log("program"," ...............................");
    running = false;
}else{


if(Parser.HandleMsg(input)){
    Console.WriteLine(await twitch.HandleKommand(Parser.getCommand('!', input), Parser.getArgs(input)));
    
}else{
   
string msg = await twitch.Write(input);
}


}

}

}



    }
}