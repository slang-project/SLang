unit Counter extend Comparable is
	unit InternalCompilerType is 
		foreign 
	end
	hidden const data: InternalCompilerType
	const zero alias nothing: InternalCompilerType
	const one: InternalCompilerType
	hidden + (other: InternalCompilerType) : as this is foreign
	hidden - (other: InternalCompilerType) : as this is foreign
	init is
		data := nothing;
	end
	init(value: InternalCompilerType)is
		data := value
	end
	succ: as this is
		ensure return > this
		return  Counter(data + one)
	end
	pred: as this =>
					if this > zero
					then 
						Counter(data - one)
					else
						raise "Zero does not have a predecessor"
	> (other: as this): Boolean =>
		if other = zero
		then
			Boolean.true
		else
			if this = zero
			then
				Boolean.false
			else
				this.pred > other.pred
end //Counter