function RECTANGLE(x, y, w, z)
	return {x, y, w, z}
end

function VECTOR3(x, y, z)
	return {x, y, z}
end

function BOOLEAN(b)
	return b
end

function STRING(s)
	return s
end

function FLOAT(f)
	return f
end

function GROUP(orders, platoon, Units)
	return {orders, platoon, Units}
end

function Sound(Bank, Cue, LodCutoff)
	return {Bank, Cue, LodCutoff}
end

categories = {
	['LAND'] = STRING('categories.LAND'),
	['NAVAL'] = STRING('categories.NAVAL'),
	['AIR'] = STRING('categories.AIR'),
	['ENGINEER'] = STRING('categories.ENGINEER'),
}
