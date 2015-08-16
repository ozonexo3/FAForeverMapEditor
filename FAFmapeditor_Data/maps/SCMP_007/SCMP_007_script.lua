local ScenarioUtils = import('/lua/sim/ScenarioUtilities.lua')

function OnPopulate()
	ScenarioUtils.InitializeArmies()
end

function OnStart(self)

#    self.Wind = Sound { Cue='Amb_Planet_Wind_01', Bank='AmbientTest' }
#    PlayLoop(self.Wind)
end
