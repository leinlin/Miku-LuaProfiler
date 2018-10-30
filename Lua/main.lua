local main = {}
local speed = 10

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
    self:update1()
end

function main:update1()
    self:update2()
end

function main:update2()
    local tb = {}

    return tb
end

function main:ondestroy()
    print("lua destroy")
end

return main

