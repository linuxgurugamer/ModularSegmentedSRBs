
Gene looked at the reports and sighed. How did it come to this... again???
Gathering everything together, he stomped into the main briefing room, looked around coldly at everyone, then slammed the pile of reports onto the table, sending papers flying all across the room.
"The budget projects for the next project are out of sight.... Again!!!"
"It's those darned SRBs, they're too big..." Bill replied meekly.
"They're still cheaper than making complete rockets" Bob continued even more softly...
Gene just glared...
"I have an idea," von Kerman raised his hand, and all eyes turned on him. "Why don't we make the SRBs the same way we make rockets; in parts"
Gene looked exasperated. "Because rockets use LIQUID FUEL!!!"
Werhner took a deep breath and smiled slyly....
"So?"
Gene rolled his eyes, "How do you intend to fuel the SRBs on the pad???  They ARE solid, you know!"
von Kerbman just smiled and continued unphased.
"Why don't me make standard sized modules, fill them with the solid propellant, and then stack them together. Just like we stack the liquid tanks"
Gene took a deep breath.  Everyone shrank back, expecting an explosion.  No explosion happened, other than Gene turning a bit greener.
"Ummm, explain yourself", Gene grumbled, then sighed in resignation. "Please".
"The thrust of an SRB is determined by how long it is." Von Kerman explained  "So, we stack segments to get the desired length... and Viola'"
Everyone watched amazed as Gene's eys crisscrossed a few times, then slowly focused back on von Kerman.
"You have a week to make it happen..." 


Features

* SRB segments, 2, 4 and 8 meters long.  One part for each diameter using PartVariants
* 3 different engine styles, two parts for each engine, PartVariants used to scale engine to different sizes
* Nosecones with built in parachutes and sepratrons, PartVariants used to scale engine to different sizes
	* Parachutes have built-in, adjustable delay
* Endcaps for the top of SRBs, needed if not using the Nosecones
* Visual alarm in editor if SRB stack is too tall, outsized SRB stack gets highlighted
* Abort mode, nosecones separate from stack, causing thrust be cancelled out by gases spewing from open stack
* Failure modes if SRBs are made too long
* Thrust directly relates to height, taller = more thrust
* Two thrust models, one which matches stock thrust, one which matches BetterSRBs (about double stock)
* Dev/test mode for testing, enables actions and PAW event to trigger engine failure
* Audible alarm when engine failure occurs
* SRB stack experiencing engine failure gets highlighted

Parts

There are 4 types of parts, one of each for the sizes 0.625m, 1.25m, 1.875m and 2.5m diameter

Nosecones w/integrated endcaps.  These nosecones have integrated sepratrons and paracutes.  Additionally each nosecone has an optional integrated fuel segment as one of the variants.

Endcaps.  These are needed to cap the top of the SRB segments when not using the nosecone, otherwise the exhaust will not be contained and vent upwards as well as downwards.

Segments (aka tanks).  There is one part for each of the 4 diameters available.  Each part can be either 1 segment long, 2 segments long or 4 segments long using the PartVariants to move between then.  The length of a segment depends on the diameter of the part:

	Diameter	Segment Length

	0.625m			1m
	1.25m			2m
	1.875m			3m
	2.5m			4m
	3.75m			6m

Motors.  There are three different motors in each size, ranging from the least capable to the most capable.  As with the nosecones, each motor has an optional fuel segment, available using the PartVariants
	
	Minuteman 
	Atlas
	STS

Fuel.  Currently there is only one type of solid fuel available.  In a future release, there may be an additional one or two fuel types added

Usage

Segments can be stacked on each other, and the part variant selected even while stacked together.  Be sure to put a motor on the bottom, and something on top, either a nosecone, or if putting other parts on top, an endcap

Thrust vs Duration

SRBs generate thrust by the burning of the solid fuel.  The more surface area is exposed inside the SRB, the more will burn and the end result will be greater thrust.
The duration of the burn, however, will depend on how thick the amount of fuel is.  So, to get a longer burning SRB, you need to make it fatter.  
Additionally, fatter SRBs will end up with a larger exposed surface on the inside, so a fatter SRB will actually have  more thrust than a skinny one of the same length.


Important Information about mass and dV

Solid fuel by definition does not flow.  In order to make this work properly, I've had to create three resources.  They are all hidden, but knowing what they are is necessary to understand some odd behaviour.
In order to allow vessels to be designed correctly, the fuel has to be in the tank segment .  In order to calcualate the dV correctly, the fuel has to be in the motor.
So what happens is that in the editor, all fuel is normally put into the motor.  When the CoM marker is shown, the fuel is moved into the tank segments,

In the flight scene, the fuel is in the tank for proper balance.  Because of this, at this time, the ISP, Thrust, TWR and Burn time are not correct in either the KSP info, MechJeb or KER.  When the SRB is running, the ISP, Thrust and TWR will be correct


Models have been made by @SuicidalInsanity
Consultative services provided by @OhioBob

Misc Notes (will be removed in final release)
=============================================

See: https://en.wikipedia.org/wiki/Space_Shuttle_Solid_Rocket_Booster
	Internal configuration of fuel
		11 point star
		double-truncated cone

You probably already know this, but generally...

     Fuel mass is  proportional to   diameter ^ 2 * length

Likewise,

     Dry mass is  proportional to  diameter ^ 2 * length

Those relationships aren't perfect, but they are close.  The reason dry mass is proportional to diameter^2 and not just diameter is because the casing wall thickness increases proportional to the diameter.

I found that the Kickback was the most realistic in regard to its mass characteristics.  So for BetterSRBs I kept the Kickback's fuel and empty mass unchanged.  Everything else was sized in proportion to it.

=============================
https://www.rocketryforum.com


https://forum.kerbalspaceprogram.com/index.php?/topic/58236-15-real-fuels-v1273/&do=findComment&comment=3670577
Yeah, the CRP is very helpful here:

Tank def: Solid         Not in the CRP, this is described in a comment in the tank defs as "Legacy"

Tank Def:  HTPB       in the CRP describes it as HTPB, described in a comment in the tank defs as "Standard stuff"

Tank Def: PBAN        in the CRP it's described as PBAN, comment in the tank defs say "Shuttle SRB etc"
PBAN is a solid rocket fuel binder. Along with a curing agent, it is the "glue" that holds the solid chemicals together and gives the grains their shape
 

Same for HNIW, NGNC, and PSPC

HNIW comment in the tank defs says: "Speculative lower-smoke propellant (not in full production yet)"

NGNC comment in the tank defs says "SPRINT missile, etc"

PSPC comment in the tank defs says "Early propellant, pre-Aluminum. Think 'Tiny Tim'"


At least in the tank definitions there is a one line comment for each fuel type, but it's really not helpful at all. In my research on this, I found the following:

Double-base (DB) propellants: DB propellants are composed of two monopropellant fuel components where one typically acts as a high-energy (yet unstable) monopropellant and the other acts as a lower-energy stabilizing (and gelling) monopropellant.
Composite propellants: A powdered oxidizer and powdered metal fuel are intimately mixed and immobilized with a rubbery binder (that also acts as a fuel).
High-energy composite (HEC) propellants: Typical HEC propellants start with a standard composite propellant mixture (such as APCP) and add a high-energy explosive to the mix (not used that much)
Composite modified double base propellants: Composite modified double base propellants start with a nitrocellulose/nitroglycerin double base propellant as a binder and add solids (typically ammonium perchlorate (AP) and powdered aluminium) normally used in composite propellants
Minimum-signature (smokeless) propellants: One of the most active areas of solid propellant research is the development of high-energy, minimum-signature propellant using C6H6N6(NO2)6 CL-20 nitroamine (China Lake compound #20), which has 14% higher energy per mass and 20% higher energy density than HMX

and the most interesting one for me:

Electric solid propellants: Electric solid propellants (ESPs) are a family of high performance plastisol solid propellants that can be ignited and throttled by the application of electric current.

high performance electric propellant (HIPEP)  modern breed of electric solid propellants was developed with higher conductivity and specific impulse.


Grain
Future addition will be adding the ability to specify a grain pattern for the srb.
URLs for reference:  
	https://www.nakka-rocketry.net/th_grain.html
	https://space.stackexchange.com/questions/4153/could-3d-printing-be-used-to-achieve-perfect-grain-geometry-of-solid-and-hybrid

	https://en.wikipedia.org/wiki/Solid-propellant_rocket#Grain_geometry
	Circular bore: if in BATES configuration, produces progressive-regressive thrust curve.
End burner: propellant burns from one axial end to other producing steady long burn, though has thermal difficulties, center of gravity (CG) shift.
C-slot: propellant with large wedge cut out of side (along axial direction), producing fairly long regressive thrust, though has thermal difficulties and asymmetric CG characteristics.
Moon burner: off-center circular bore produces progressive-regressive long burn, though has slight asymmetric CG characteristics
Finocyl: usually a 5- or 6-legged star-like shape that can produce very level thrust, with a bit quicker burn than circular bore due to increased surface area.


Current Issues
==============

Check drag cubes for all chutes - still needs to be done

Manufacturer not showing up
Not all tech parts in tree