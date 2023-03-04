#include <windows.graphics.h>
#include <windows.graphics.capture.h>
#include <windows.graphics.capture.interop.h>

//#include <winrt/windows.graphics.h>
#include <winrt/Windows.Graphics.Capture.h>

//#include <winrt/windows.graphics.capture.h>

extern "C" _declspec(dllexport) int AddCustom(int a, int b);

int AddCustom(int a, int b) {
	return a + b;
}

extern "C" _declspec(dllexport) winrt::Windows::Graphics::Capture::IGraphicsCaptureItem GetDevTest(HWND hwnd);

winrt::Windows::Graphics::Capture::IGraphicsCaptureItem GetDevTest(HWND hwnd) {
	auto activation_factory = winrt::get_activation_factory<winrt::Windows::Graphics::Capture::GraphicsCaptureItem>();
	auto interop_factory = activation_factory.as<IGraphicsCaptureItemInterop>();
	winrt::Windows::Graphics::Capture::GraphicsCaptureItem item = { nullptr };
	interop_factory->CreateForWindow(hwnd, winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(), reinterpret_cast<void**>(winrt::put_abi(item)));
	return item;
}


extern "C" _declspec(dllexport) HWND GetDevTest2(HWND hwnd);

HWND GetDevTest2(HWND hwnd) {
	return hwnd;
}
