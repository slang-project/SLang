
abstract unit Routine [Arguments->(), Result] is
	arguments: as Arguments is foreign
	()(args: Arguments) is abstract
	()(args: Arguments): Result is abstract
end

unit Procedure [Arguments->()] extend Routine [Arguments, ()] is
	override () (args: Arguments) is foreign
	override hidden ()(args: Arguments): Result is end
end

unit Function [Arguments->(), Result] extend Routine [Arguments, Result] is
	override hidden () (args: Arguments) is end
	override ()(args: Arguments): Result is foreign
end