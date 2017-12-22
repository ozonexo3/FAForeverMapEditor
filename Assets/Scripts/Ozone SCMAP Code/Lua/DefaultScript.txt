local ScenarioUtils = import('/lua/sim/ScenarioUtilities.lua')
local ScenarioFramework = import('/lua/ScenarioFramework.lua')

function OnPopulate()
    ScenarioUtils.InitializeArmies()
    ScenarioFramework.SetPlayableArea('AREA_1' , false)
end

function OnStart(scenario)
end