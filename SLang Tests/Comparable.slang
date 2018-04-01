abstract unit Comparable is
	routine < (other: as this): Boolean is abstract
	// It looks we may rely on default equality test from Any ...
	/*override abstract = (other: as this): Boolean
	end*/
	routine > (other: as this): Boolean => other < this
	routine <= (other: as this): Boolean => not (other < this)
	routine >= (other: as this): Boolean => not (this < other)
	routine max (other: as this): as this => if this > other then this else other
	routine min (other: as this): as this => if this < other then this else other
invariant
	not (this < this) /// Irreflexive comparison
end // Comparable