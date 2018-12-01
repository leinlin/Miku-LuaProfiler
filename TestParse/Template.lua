
--[[]]local self.xx = function()
end

self['x1'] = function()
end

local self:x2 = function()
end

local self.x3 = function()
end

local xx = function()

end

function()
    return
end

function()
    return function()print("fk")end
end

function()
    return fk;
end

function()
    return fk--fkyou
end

function()
    return fk       --fkyou
end

function()
    return {
	--fkyou
	fk,
	--fkyou
	fk1,
	}

end

function()
    return fk(--[[]]1)
end

function()
    return fk
--fkyou
end

function() fk()end

function (prototype, data)
    return setmetatable(data, pb.defaults(prototype))
        -- {
        --     __index = pb.defaults(prototype),
        --     __newindex = function() error("Xlsx", "Attempt to modify read-only table") end
        -- })
end

function()
    return KuafuModule
end


return {}