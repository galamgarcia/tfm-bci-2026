/*
 * BCI Interaction System for Videogames
 * Master's Thesis · University of Alicante
 * © 2026 Gala M. García
 */

package bit.brainlink;

import com.boby.bluetoothconnect.LinkManager;
import com.boby.bluetoothconnect.classic.listener.OnReceiveBytesListener;
import com.macrotellect.unityforandroidsdk.UnitySDK;
import com.unity3d.player.UnityPlayer;

import java.lang.reflect.Field;

/**
 * Publishes BrainLink eye samples that the bundled Android parser silently drops.
 *
 * The BrainLink Android SDK already receives the Bluetooth bytes and owns the normal listeners. 
 * This bridge wraps the existing byte listener, observes the same data, and forwards only the requested value to Unity.
 * The packet format is {@code AA AA 04 80 02 00 XX}, where {@code XX} is part of a signed two-byte eye sample.
 */
 public final class BrainLinkBridge {
    // Prevents installing a second wrapper around the SDK listeners.
    private static boolean isInstalled;

    // This utility class is not instantiated; Unity calls its static entry point.
     private BrainLinkBridge() { }

    /**
     * Installs observers on both Bluetooth transports without replacing the SDK parser.
     * @param objectName Unity GameObject receiving the ReceiveBlink callback.
     */
    public static synchronized void install(String objectName) {
        // Persistent Unity objects can call Start more than once across scene loads.
        if (isInstalled) { return; }

        try {
            // UnitySDK is the SDK facade used by the existing UnityThinkGear wrapper.
            UnitySDK sdk = UnitySDK.getInstance();
            // linkManager is private SDK state, so reflection is required because the proprietary SDK exposes no public byte-listener accessor.
            Field managerField = UnitySDK.class.getDeclaredField("linkManager");
            managerField.setAccessible(true);
            LinkManager manager = (LinkManager) managerField.get(sdk);
            // A null manager means the SDK has not initialized its Bluetooth layer.
            if (manager == null) { return; }

            // BrainLink devices may use either transport depending on the SDK path
            installOnService(manager, "classicService", objectName);
            installOnService(manager, "bleService", objectName);
            isInstalled = true;
        } catch (Exception exception) {
             android.util.Log.e("BrainLinkBridge", "Unable to install blink listener", exception);
        }
    }

    /**
     * Wraps one internal SDK service listener and preserves its original callback.
     * @param manager SDK Bluetooth manager containing the service.
     * @param serviceFieldName private service field to inspect.
     * @param objectName Unity GameObject receiving the callback.
     * @throws ReflectiveOperationException when an expected SDK field is unavailable.
     */
    private static void installOnService(LinkManager manager, String serviceFieldName, String objectName) throws ReflectiveOperationException {
        // Locate the selected Bluetooth service inside LinkManager.
        Field serviceField = LinkManager.class.getDeclaredField(serviceFieldName);
        serviceField.setAccessible(true);
        Object service = serviceField.get(manager);
        // A service can be null when that transport is not active on this device.
        if (service == null) { return; }

        // The SDK stores its byte callback in this private field.
        Field listenerField = service.getClass().getDeclaredField("bytesListener");
        listenerField.setAccessible(true);
        OnReceiveBytesListener original = (OnReceiveBytesListener) listenerField.get(service);
        // Do not install a wrapper that cannot forward to an existing listener.
        if (original == null) { return; }

        listenerField.set(service, new OnReceiveBytesListener() {
            // Each transport keeps parser state independently because callbacks can split packets at different byte boundaries.
            private final PacketReader reader = new PacketReader(objectName);

            @Override
            public void onReceive(byte[] data, int length) {
                // Preserve all vendor behavior before applying the custom observer.
                original.onReceive(data, length);
                reader.read(data, length);
            }
        });
    }

    /** Incrementally parses the byte stream delivered by one Bluetooth service. */
    private static final class PacketReader {
        // Unity target used by UnitySendMessage when a sample is recognized.
        private final String objectName;
        // Maximum payload accepted from the one-byte packet-length field.
        private final byte[] payload = new byte[255];
        // Current position in the packet state machine.
        private int state;
        // Number of payload bytes expected after the packet header.
        private int payloadLength;
        // Number of payload bytes already copied into payload.
        private int payloadIndex;

        /**
         * Creates a parser associated with one Unity callback target.
         * @param objectName Unity GameObject receiving recognized samples.
         */
        PacketReader(String objectName) {
            this.objectName = objectName;
        }

        /**
         * Consumes one callback from the SDK without assuming packet alignment.
         * @param data bytes supplied by the SDK.
         * @param length number of valid bytes in data.
         */
        void read(byte[] data, int length) {
            // Clamp the SDK-provided length to the actual Java array size.
            int size = Math.min(length, data.length);
            for (int index = 0; index < size; index++) {
                // Convert Java's signed byte to an unsigned protocol byte (0..255).
                readByte(data[index] & 0xff);
            }
        }

        /**
         * Advances the parser by one unsigned byte.
         * @param value unsigned byte value from the Bluetooth stream.
         */
        private void readByte(int value) {
            switch (state) {
                case 0:
                    // Search for the first synchronization byte AA.
                    state = value == 0xaa ? 1 : 0;
                    return;
                case 1:
                    // Require two consecutive AA bytes before accepting a packet.
                    if (value == 0xaa) {
                        state = 2;
                    } else {
                        // The candidate header was invalid; resume searching.
                        state = 0;
                    }
                    return;
                case 2:
                    // The next byte declares how many payload bytes follow.
                    payloadLength = value;
                    payloadIndex = 0;
                    // Reject zero or oversized lengths before writing into payload.
                    state = payloadLength > 0 && payloadLength <= payload.length ? 3 : 0;
                    return;
                case 3:
                    // Copy one payload byte and wait until the declared length is met.
                    payload[payloadIndex++] = (byte) value;
                    if (payloadIndex >= payloadLength) {
                        // The next byte is treated as the checksum/packet terminator.
                        state = 4;
                    }
                    return;
                default:
                    // The payload is complete. Interpret it before searching for the
                    // next AA header; this also supports back-to-back packets.
                    publishEyeValue();
                    state = 0;
                    if (value == 0xaa) {
                        // Reuse the current byte as the first byte of a new header.
                        state = 1;
                    }
                    return;
            }
        }

        /**
         * Publishes a recognized eye sample and ignores unrelated payloads.
         */
        private void publishEyeValue() {
            // Proprietary BrainLink packet observed in Android LinkManager logs.
            if (payloadLength == 4 && (payload[0] & 0xff) == 0x80 && (payload[1] & 0xff) == 0x02) {
                // Rebuild the big-endian 16-bit sample from its two payload bytes.
                int intensity = (short) (((payload[2] & 0xff) << 8) | (payload[3] & 0xff));
                intensity = Math.abs(intensity);
                // Serialize the integer 
                UnityPlayer.UnitySendMessage(objectName, "ReceiveBlink", Integer.toString(intensity));
                return;
            }

            // Fallback parser for the standard ThinkGear payload convention.
            int index = 0;
            while (index < payloadLength) {
                // A normal code occupies one byte and defaults to one value byte.
                int code = payload[index++] & 0xff;
                int valueLength = 1;
                if (code > 0x7f) {
                    // Extended codes carry an additional code byte and an explicit value length. Stop safely if the payload is truncated.
                    if (index >= payloadLength) { return; }
                    code = ((code & 0x7f) << 8) | (payload[index++] & 0xff);
                    if (index >= payloadLength) { return; }
                    valueLength = payload[index++] & 0xff;
                }

                // Never read beyond the payload declared by the packet.
                if (index + valueLength > payloadLength) { return; }
                if (code == 0x08 && valueLength == 1) {
                    // Code 0x08 is the standard one-byte eye/blink value.
                    UnityPlayer.UnitySendMessage(objectName, "ReceiveBlink", Integer.toString(payload[index] & 0xff));
                }
                // Skip this field and continue looking for another field.
                index += valueLength;
            }
        }
    }
}
