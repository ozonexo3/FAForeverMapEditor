local ScenarioUtils = import('/lua/sim/ScenarioUtilities.lua')

function OnPopulate()
	ScenarioUtils.InitializeArmies()
end

function OnStart(self)
end