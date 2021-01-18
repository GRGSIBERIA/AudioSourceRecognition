import matplotlib.pyplot as plot 

plot.figure()

lines = []
with open("./Fourier.txt", "r", encoding="sjis") as r:
    lines = r.readlines()

Y = []
for line in lines:
    datum = line.split(",")
    Y.append(float(datum[1]))

plot.plot(Y)
plot.tight_layout()
plot.show()