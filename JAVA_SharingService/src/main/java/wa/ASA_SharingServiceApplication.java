package wa;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

@SpringBootApplication
public class ASA_SharingServiceApplication {

	public static void main(String[] args) {
		SpringApplication.run(ASA_SharingServiceApplication.class, args);
	}

	@Bean
	CommandLineRunner init() {
		return (evt) -> {
			System.out.println("AnchorSharing started sucsessful.");
		};
	}
}
