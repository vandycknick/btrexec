.DEFAULT_GOAL := default

ARTIFACTS 			:= $(shell pwd)/artifacts
BUILD				:= $(shell pwd)/.build
TEMP				:= $(shell pwd)/.tmp
CONFIGURATION		:= Release
LIB					:= src/SubProcess/Nvd.SubProcess.csproj
UNIT_TESTS			:= test/SubProcess.Test/Nvd.SubProcess.Test.csproj

SANDBOX_EMULATOR	:= sandbox/Emulator/Emulator.csproj

.PHONY: default
default:
	$(MAKE) package

.PHONY: package
package:
	dotnet pack $(LIB) \
		--configuration $(CONFIGURATION) \
		--output $(ARTIFACTS)

.PHONY: test
test:
	dotnet test $(UNIT_TESTS) -c $(CONFIGURATION) \
		/property:CollectCoverage=true \
		/property:CoverletOutputFormat=lcov \
		/property:CoverletOutput=$(TEMP)/nvd.subprocess.test/lcov.info

.PHONY: run-native
run-native:
	cd src/NativeHelper && $(MAKE) 

.PHONY: sandbox-3mulator
sandbox-emulator:
	dotnet run -p $(SANDBOX_EMULATOR)
	