abstract unit Comparable is
	> (other: as this): Boolean is abstract
	// It looks we may rely on default equality test from Any ...
	/*override abstract = (other: as this): Boolean
	end*/
	< (other: as this): Boolean => other > this
	<= (other: as this): Boolean => not (other < this)
	>= (other: as this): Boolean => not (this < other)
	max (other: as this): as this => if this > other then this else other
	min (other: as this): as this => if this < other then this else other
invariant
	not (this < this) /// Irreflexive comparison
end // Comparable