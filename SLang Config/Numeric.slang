abstract unit Numeric is
	one: as this is abstract
		/// Definition of One
	zero: as this abstract
		/// Definition of Zero
	+ (other: as this): as this is abstract
		/// Definition of addition
	ensure
		this - other = old this
	end // +
	- (other: as this): as this is abstract
		/// Definition of subtraction 
	ensure
		this + other = old this		
	end // -
	- : as this is abstract
		/// Definition of negation
	ensure
		this - old = this
	end
	/ (other: as this): as this is abstract
		/// Definition of division
	end
	* (other: as this): as this is abstract
		/// Definition of multiplication
	end
	** (other: as this): as this is abstract
		/// Definition of power
	end
invariant
	this = this * one  /// Multiplication constituent
	this = this + zero /// Addition constituent
	this = this ** one  /// Power constituent first
	one = this ** zero  /// Power constituent second
	zero = this - this /// Zero constituent
end // Numeric