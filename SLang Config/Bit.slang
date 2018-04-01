//оператор равно = определён? и что же всё-таки делать с 0b0, 0b1 ? - хочется, чтобы компилятор их сам задавал
val unit Bit
	Bit CF := 0b0
    const data is
         0b0, 0b1
    end
	& alias and (other: as this): as this => if this = 0b0 then 0b0 elsif other = 0b0 then 0b0 else 0b1

	| alias or (other: as this): as this => if this = 0b1 then 0b1 elsif other = 0b1 then 0b1 else 0b0

	^ alias xor (other: as this): as this => if this = other then 0b0 else 0b1
		
	~ alias not : as this => if this = 0b0 then	0b1 else 0b0
		
	+ (other: as this): as this is
		CF := 0b0
		/// Definition of addition
		if this = 0b0 then
			return other
		elsif other = 0b1 then
			return 0b1
		else
			CF := 0b1 // "Bit overflow"
			return 0b0
		end
	end // +
	+= (other: as this): as this => this + other
	
	- (other: as this): as this is
		CF := 0b0
		/// Definition of subtraction 
		if this = other 
		then
			return 0b0
		else
			if this = 0b1 
			then
			return 0b1
			else
				CF := 0b1 // "Bit underflow"
				return 0b1
			end
		end
	end // -
	/= (other: as this): as this => not this = other
end
