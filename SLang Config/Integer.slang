///implementation is based on Two's complement little-endian binary representation
//the unit itself is not of arbitrary precision, but if overflow happens - it returns the integer of wider bitscale
val unit Integer [BitsNumber: Counter] extend Numeric, Enumeration is
	const minInteger is - (2 ** (BitsNumber - 1))
	const maxInteger is 2 ** (BitsNumber - 1) - 1
	const is /* That is ordered set defined as a range of all Integer constant values (objects) */
		minInteger .. maxInteger
	end
	override const zero is Integer[BitsNumber] // по умолчанию 
	override const one is // 0b1 вот здесь я хочу сказать, что это объект с битовой шкалой, где на последнем месте стоит 0b1
	//я понимаю, что 'one' это не функция, но мне каким-то образом нужно rightmost бит в шкале поменять на единицу 
	//(все остальные нули по умолчанию)
		bitScale is Bit[BitsNumber] //по умолчанию все биты равны нулю
		bitScale(0) := 0b1 //the rightmost bit becomes 0b1

		oneConst is Integer[BitsNumber]
		oneConst := bitScale //calling conversion ctr with Bit[]

		return oneConst
	end
	//наверное, всё-таки Real? - размер не всегда целое или нужно делать инвариант -> BitsNumber кратно Platform.BitsInByteCount
	override sizeof: Integer is
		return BitsNumber / Platform.BitsInByteCount
	end
	succ: as this => this + one // ex.: 2 = one + one = 0b1 + 0b1 = bit нам вернет 0b0 и cf = true 
	pred: as this => this - one	
	override const first is minInteger
	override const last is maxInteger
	count: Integer => last - first + 1
	ord:  Integer => this - minInteger + 1
	override + (other: as this): as this is //я ожидаю, что as this - это Integer в целом (любое количество бит)
		widestBitsNumber is this.BitsNumber
		if widestBitsNumber < other.BitsNumber
		then
			widestBitsNumber = other.BitsNumber
		end

		res is Integer[widestBitsNumber+1] //здесь должны быть по дефолту везде нули, N+1 - для одного доп бита переполнения
		while pos in 0 .. BitsNumber-2
		loop
			res(pos) += this(pos) + other(pos)
			if res(pos).CF = 0b1 
			then
				res(pos+1) := 0b1
				//else res(pos+1) is 0b0 by default
			end
		end
		if res(BitsNumber-1) = 0b1 //overflow of the scale (more than )
			or
			this(BitsNumber-1) and other(BitsNumber-1) xor res(BitsNumber-2) = 0b1 
			//if summing two positives(negatives) give a negative(positive) number => overflow
		then
			//raise "Overflow exception"
			//давайте всё-таки возвращать бóльшую битовую шкалу
			return res 
		end

		resultingInteger is Integer[widestBitsNumber]
		resultingInteger.data := res //здесь мы поместим всё, что уместилось справа-налево (останется верное значение с первоначальным количеством бит)
		return resultingInteger
	end
	- is
		if other is zero 
		then
			return zero
		else
			if this is minInteger
			then
				//there is no positive Integer with the value = |minInteger| in "Two's complement" with such range of bit scale
				res is Integer[BitsNumber+1](this)
				return -res
			else
				//two's complement to convert the number
				res is this.toggle
				return res + one
			end
		end
		Как мы его вызываем спереди объекта или после? Вот так: -12 или так 12-
		Если да, то как описывать пост/пред оператор?
		// Unary minus
		return zero - this
	end
	override - (other: as this): as this is
		return this+(-other)
	end
	override / (other: as this): as this is
		//implemented with 'Non-restoring Division Algorithm'
		A is Integer(this) //dividend, later the quotient will be stored in A
		P is Integer[this.BitsNumber+1] // later the remainder will be stored in P
		B is other.align(P.BitsNumber) //devisor

		while pos in 0 .. P.BitsNumber
		loop
			//1. shifting bit pair (P,A)
			P<<1
			P(0) := A(A.BitsNumber-1)
			A<<1
			//2.
			if P.signum < 0
			then
				P := P+B
			else
				P := P-B
			end
			//3.
			if P.signum < 0
			then
				A(0):=0b0
			else
				A(0):=0b1
			end
		end
		return quotient
		
	end
	/ (other: as this): as this is
		// Bit-level implementation of Integer division reminder
		raise "Not implemented!!!"
	end
	override * (other: as this): as this is
		//мы здесь хотим чтобы число помещалось в Integer[BitsNumber] или нет?

		widestBitsNumber is this.BitsNumber //it will be our x which is equal to y
		if widestBitsNumber < other.BitsNumber
		then
			widestBitsNumber = other.BitsNumber
		end

		m is this.align(widestBitsNumber) // multiplicand
		neg_m is -m //еще раз выхождение за пределы range
		r is other.align(widestBitsNumber) // multiplier

		workingLength is 2*widestBitsNumber+1
		A is Integer[workingLength]
		S is Integer[workingLength]
		P is Integer[workingLength]

		//1. initializing values
		while pos in 0 .. widestBitsNumber - 1
		loop
			A(widestBitsNumber + pos) := m(pos)
			S(widestBitsNumber + pos) := neg_m(pos)
			P(pos+1) := r(pos)
		end

		while i in 0 .. widestBitsNumber
		loop
			//2. Changing P
			if P(0) xor P(1) = 0b1
			then
				if (P) = 0b0
				then
					P += A
				else
					P += S
				end
			end

			//3. Arithmtetic shift
			P := P>>1
		end

		result is Integer[2*BitsNumber]
		while pos in 0 .. BitsNumber-1
		loop
			result(pos) := P(pos+1)
		end
		return result
	end
	override ** (other: as this): as this is
		// Bit-level implementation of Integer rasing to power
		if other = zero
		then
			return one
		else
			if other = 1
			then
				return this
			else
				return this * (this**(other-1))
			end
		end
	end
	override < (other: as this): Boolean isw
		 //leftmost bit for sign: 0b0 is Positive, 0b1 is Negative
		 if this(BitsNumber-1)/=other(BitsNumber-1) //numbers have different signs
		 then
		 	return this(BitsNumber-1)>other(BitsNumber-1) // 0b0(plus-sign) is bigger than 0b1(minus-sign)
		 else
		 	while i in 2..BitsNumber loop
		 		if this(i) /= other(i)
		 		then
		 			return this(i)<other(i)
		 	end
		 	return false
		 end

	end
	override = (other: as this): Boolean is
		// Bit-level implementation of Inetegr equal
		return data = other.data
	end
	override := (other: as this) is
		init (other)
	end
	//для шкал с другим размером
	override := (other: as this) is
		init (other)
	end
	:= (other: Character) is
		// Bit-level implementation of Chracter assignment
	raise "Not implemented!!!"
		if data.sizeof = other.data.sizeof then
			data := other.data // Size of data is different!!!
		else
		end
	end
	toCharacter: Character
	require
		this in 0 .. 2 ** Platform.CharacterBitsCount /// containsChacraterCode: 
	is
		return Character.init (data.getLowBits (Platform.CharacterBitsCount))
	end
	signum : Integer[BitsNumber] is
		if this.data(BitsNumber-1) = 0b1 
		then
			return -one // вообще говоря, могу вернуть сразу интеджер с 2мя битами "11" - это как раз -1
		else 
			if this.data = 0b0
			then
				return 0b0 //потом можно вызвать conversoin ctr, который за-align-ит к текущему количеству бит 
			else
				return 0b1 //потом можно вызвать conversoin ctr, который за-align-ит к текущему количеству бит
			end
		end
	end
	//aligns the scale with the bigger number of bits
	align(bitsNumber: Counter): as this is
		if this.BitsNumber > bitsNumber
		then
			raise "Cannot align the scale to lower number of bits"
		else
			widerData is Bit[bitsNumber]
			widerData := this // приведение Integer'a к битовой шкале по умолчанию - leftmost bits are 0b0s by default
			if this.signum /= 0b0 | this.signum /= 0b1  //значит -> число отрицательное
				and
				this.BitsNumber < bitsNumber // если есть еще доп биты слева
			then
				while pos in this.BitsNumber .. bitsNumber - 1
				loop
					widerData.toggle(pos) //toggling 0b0s to 0b1s according to "Two's complement" to make it negative
				end
			end
			return widerData
		end
	end
	init is
		/// Default init
		data := Bit [BitsNumber] //by default all bits are 0b0
	end
	init (bitScale: Bit[]) is
		data := bitScale
	end
	init (bit: Bit) is
		data := Bit [BitsNumber]
		if bit = 0b0
		then
			data := zero
		else
			data := one
		end
	end
	init (other: as this) is
		data := Bit [BitsNumber]
		if this.BitsNumber < other.BitsNumber
		then
			raise "Cannot convert Integer with wider range to Integer with smaller one"
		else
			otherWider := other.align(this.BitsNumber) // выравниваем шкалу rvalue по шкале lvalue
			data := otherWider
		end
	end
	hidden data: Bit [BitsNumber]
invariant
	BitsNumber > 2 /// Number of bits in Integer must be greater than 2: one valuable bit, one sign bit
end // Integer
