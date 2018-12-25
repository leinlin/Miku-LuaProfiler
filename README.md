## Lua Profiler For Unity
<br/>

### Purpose
**Unity + Lua** script is now most popular incremental update frameWork for mobile game in China , But because that not has an **easy-to-use profiler** cause more cpu usage to lua gc and also more memory usage . so much so that programmers don't know how to optimize their game . Then they replace lua code with C# to find the problems , but also more buildin code and less flexibility in operation activity .<br>
so this tool is designed to support an **easy-to-use profiler** for unity that help finding battleneck and make your game more fast and stable.

## 
If you want to test this project,use administrator mode to run the **link.bat** <br/>

##

### Contact
If you find any bug or have any suggests join the QQ group：[882425563](https://jq.qq.com/?_wv=1027&k=5QkOBSc) to contact us

### Deploy and Install
This tool now supports **XLua**、**SLua**、**ToLua**.This is a remote profiler tool so it supports **Windows**、**Android**、**IOS** On-device Profiler.

- Open **LuaProfiler** folder
- Copy **LuaProfilerClient** folder to you game project content,if your C# Lua script is in **Plugins** folder,Copy **LuaProfilerClient** to **Plugins**.This Tool must make sure That code must in the same DLL which has C# lua codes.
- Use **Unity5.6 or newer** unity version to create a project, copy **LuaProfilerServer** to the project content

### Theory
It use mono.ceil's IL inject feature(also use in XLua),inject the profiler code to game code

## 

### Tutors

you must open two unity projects,one for game client ,one for editor server

#### Config your client

Open windows by **"Window->Lua Profiler Window"**, toggle profiler's feature and configure the server ip address.
## 
![](doc/config_client.png)

#### Open server
Also open windows by **"Window->Lua Profiler Window"**, then click **OpenService**,wait for client connects
## 
![](doc/config_server.png)

## 
![](doc/profiler.gif)
## 

#### Record mode
Click **Record** button, when game connect to server, toggle **StartRecord** to start or stop record.

##### Record button feature


- drag slider to modify samples
- click __'<'__ 、 __'>'__ to increase or discrease frames one by one
- click __'<<'__ 、 __'>>'__ to fast locate the frames control by 
**Capture Lua GC**、**Capture Mono GC**、**Frame Count**

##
![](doc/record.gif)

### On-device Profiler
Set macro **USE_LUA_PROFILER** to inject profiler code in you App.If you want to use **luac code or luajit bytecode** ,use **InjectLua.exe** in folder tools To inject the lua profiler code.

```
InjectLua.exe "inpath" "outpath"
```

### Use Case
![](doc/ljjc.jpg)

## 
### Thanks
[easy66](https://github.com/easy66) <br/>
[Xavier](https://github.com/starwing) <br/>
[Jay](https://github.com/Jayatubi) <br/>
[ZhangDi](https://github.com/ZhangDi2018) <br/>
and all members in qq group [LuaProfiler](https://jq.qq.com/?_wv=1027&k=5QkOBSc)

## 
![](doc/meizi.gif)
