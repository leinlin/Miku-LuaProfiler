## Lua Profiler For Unity
<br/>

### Purpose
****Unity** + Lua** script is now most popular I**ncremental Update FrameWork** For Mobile Game In China,But Becase that not has a **easy-to-use profiler** cause many game more cpu usage to lua gc and also more memory usage. so much so that programmers don't know how to optimize their game.then they replace lua code with C# to find the problems,but also more buildin code and less flexibility in operation activity<br>
so this tool is design to support a **easy-to-use profiler** for unity that help the game which use the lua script more early to **officially online** and **stable operations**

## 
If you want to test this project,use administrator mode to run the **link.bat** <br/><br/>
If you find **any bug** or has **any suggest** join the **qq group**：[882425563](https://jq.qq.com/?_wv=1027&k=5QkOBSc) to contact us

### Deploy and Install
This tool now support **XLua**、**SLua**、**ToLua**.This is a remote profiler tool so it support **Windows**、**Android**、**IOS** On-device Profiler。

- Open **LuaProfiler** folder
- Copy `LuaProfilerClient` folder to you game project content,if your C# Lua script is in **Plugins** folder,Copy `LuaProfilerServer` to **Plugins**.This Tool must confirm That Code must in the same DLL which has C# Lua Scirpt.
- Use **Unity5.6 or newer** unity version to create a project, Copy `LuaProfilerServer` To the project content

### Theory
It use mono.ceil's IL Inject feature(also use in XLua),Inject The profiler Code to Game Code

## 

### Tutors

#### Config your Client

Open windows by **"Window->Lua Profiler Window"**, toggle profiler's feature and configure the server ip address.
## 
![](doc/config_client.png)

#### Open server
Also Open windows by **"Window->Lua Profiler Window"**, then OpenService,wait for client connects
## 
![](doc/config_server.png)

## 
![](doc/profiler.gif)
## 

#### Record mode
Click **Record** button, when Game connect to server, Toggle **StartRecord** To Start Or Stop Record.

##### Record button feature


- drag slider to modify samples
- click __'<'__ 、 __'>'__ to increase or discrease frames one by one
- click __'<<'__ 、 __'>>'__ to fast locate the frames control by 
**Capture Lua GC**、**Capture Mono GC**、**Frame Count**

<br/>

#### On-device Profiler
Set Pack Macro **USE_LUA_PROFILER** to Inject Profiler code in you App.if you want use **luac code or luajit bytecode** ,Use **InjectLua.exe** in folder Tools To Inject the lua profiler code.

```
InjectLua.exe "inpath" "outpath"
```

#### Use Case
![](doc/ljjc.jpg)

## 
### Thanks
[easy66](https://github.com/easy66) <br/>
[Xavier](https://github.com/starwing) <br/>
[Jay](https://github.com/Jayatubi) <br/>
[ZhangDi](https://github.com/ZhangDi2018) <br/>
and all members in qq group [LuaProfiler](https://jq.qq.com/?_wv=1027&k=5QkOBSc)
