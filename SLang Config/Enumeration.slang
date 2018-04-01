abstract unit Enumeration extend Comparable is
	succ: as this is abstract
	/// successor of the current object
	ensure
		return > this
	end
	pred: as this is abstract
	/// predecessor of the current object
	ensure
		return < this
	end
	first: as this is abstract
	/// first element of enumeration
	last: as this is abstract
	/// last element of enumeration
	count: Counter is abstract
	/// number of elements in the enumeration
	ord: Counter is abstract
	ensure
		return > 0		
	end
invariant 
	succ > pred
	count >= 0
	ord > pred.ord
	ord < succ.ord
end