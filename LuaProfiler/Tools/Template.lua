local BeginMikuSample = MikuLuaProfiler.LuaProfiler.BeginSample local EndMikuSample = MikuLuaProfiler.LuaProfiler.EndSample local miku_unpack_return_value = miku_unpack_return_value BeginMikuSample("[lua]:require Template.lua &line:1") 
--[[]]local self.xx = function() BeginMikuSample("[lua]:self.xx Template.lua&line:2") 
 EndMikuSample() end

self['x1'] = function() BeginMikuSample("[lua]: Template.lua&line:5") 
 EndMikuSample() end

local self:x2 = function() BeginMikuSample("[lua]:self:x2 Template.lua&line:8") 
 EndMikuSample() end

local self.x3 = function() BeginMikuSample("[lua]:self.x3 Template.lua&line:11") 
 EndMikuSample() end

local xx = function() BeginMikuSample("[lua]:xx Template.lua&line:14") 

 EndMikuSample() end

function() BeginMikuSample("[lua]: Template.lua&line:18") 
     return miku_unpack_return_value() 
end

function() BeginMikuSample("[lua]: Template.lua&line:22") 
     return miku_unpack_return_value( function() BeginMikuSample("[lua]: Template.lua&line:23") print("\u888fk") EndMikuSample() end
) end

UIManager.ShowMsgBox({content="中途退出则挑战失败\n确认是否退出？",okFunc=function() BeginMikuSample("[lua]:okFunc Template.lua&line:26") 
    GameControllers.Dungeon:dungeonCommLeave(0)
 EndMikuSample() end,title_ok="退  出",
cancelFunc=function() BeginMikuSample("[lua]:cancelFunc Template.lua&line:29") 
    GameControllers.Dungeon.isForceLeave = false
 EndMikuSample() end})

function() BeginMikuSample("[lua]: Template.lua&line:33") 
     return miku_unpack_return_value( fk) ;
end

function() BeginMikuSample("[lua]: Template.lua&line:37") 
     return miku_unpack_return_value( fk) --fkyou
end

function() BeginMikuSample("[lua]: Template.lua&line:41") 
     return miku_unpack_return_value( fk)        --fkyou
end

function() BeginMikuSample("[lua]: Template.lua&line:45") 
     return miku_unpack_return_value( {
	--fkyou
	fk,
	--fkyou
	fk1,
	}) 

end

function() BeginMikuSample("[lua]: Template.lua&line:55") 
     return miku_unpack_return_value( fk(--[[]]1)) 
end

function() BeginMikuSample("[lua]: Template.lua&line:59") 
     return miku_unpack_return_value( fk) 
--fkyou
end

function() BeginMikuSample("[lua]: Template.lua&line:64")  fk() EndMikuSample() end

function (prototype, data) BeginMikuSample("[lua]: Template.lua&line:66") 
     return miku_unpack_return_value( setmetatable(data, pb.defaults(prototype))) 
        -- {
        --     __index = pb.defaults(prototype),
        --     __newindex = function() error("Xlsx", "Attempt to modify read-only table") end
        -- })
end

function() BeginMikuSample("[lua]: Template.lua&line:74") 
     return miku_unpack_return_value( KuafuModule) 
end

if true then
	 return miku_unpack_return_value( {}) ;
end

 return miku_unpack_return_value( Mathf) ; 