local ScenarioUtils = import('/lua/sim/ScenarioUtilities.lua')
local ScenarioFramework = import('/lua/ScenarioFramework.lua')

function OnPopulate()
	ScenarioUtils.InitializeArmies()
	--ScenarioFramework.SetPlayableArea(ScenarioUtils.AreaToRect('AREA_1'))
end

function OnStart(self)
end
