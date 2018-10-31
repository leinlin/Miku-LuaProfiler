local main = {}
local speed = 10
local tick = 0;

function main:start()
    local lightObject = self.lightObject

    print("lua start...")
    print("injected object", lightObject)
    self.lightCpnt = lightObject:GetComponent(typeof(CS.UnityEngine.Light))
end

function main:update()
    local r = CS.UnityEngine.Vector3.up * CS.UnityEngine.Time.deltaTime * speed
    self.transform:Rotate(r)
    self.lightCpnt.color = CS.UnityEngine.Color(CS.UnityEngine.Mathf.Sin(CS.UnityEngine.Time.time) / 2 + 0.5, 0, 0, 1)

    for _  = 1,10 do
        self:update1()
    end

end

function main:update1()
    local t1,t2 = math.modf(tick/2);

    if(t2 == 0) then
        self:update2()
    else
        self:update3()
    end
    tick = tick + 1
end

function main:update3()
    return {},"1443","6666"
end

function main:update2()
    return {},"1443","6666"
end

function main:ondestroy()
    print("lua destroy")
end

return main

